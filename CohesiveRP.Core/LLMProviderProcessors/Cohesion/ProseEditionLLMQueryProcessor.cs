using System.Text.RegularExpressions;
using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Common.Utils.Parsers.BusinessObjects;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.Cohesion
{
    public class ProseEditionLLMQueryProcessor : LLMQueryProcessor
    {
        public ProseEditionLLMQueryProcessor(
            ChatCompletionPresetType completionPresetType,
            BackgroundQuerySystemTags tag,
            BackgroundQueryDbModel backgroundQueryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService,
            ISummaryService summaryService) : base(
                completionPresetType,
                tag,
                backgroundQueryDbModel,
                contextBuilderFactory,
                promptContextElementBuilderFactory,
                storageService,
                httpLLMApiProviderService,
                summaryService)
        { }

        public override async Task<bool> ProcessCompletedQueryAsync()
        {
            if (!await base.ProcessCompletedQueryAsync())
            {
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                backgroundQueryDbModel.RetryCount++;
                return false;
            }

            try
            {
                string LLMMessageResult = messages.First().Content;

                string LLMReplyJsonContent = LLMResponseParser.ParseOnlyJson(messages.First().Content);
                FindReplaceResultDto validJson = null;
                try
                {
                    validJson = JsonCommonSerializer.DeserializeFromString<FindReplaceResultDto>(LLMReplyJsonContent);
                } catch (Exception e)
                {
                    LoggingManager.LogToFile("b31cea09-ee6e-474c-8f6d-4652cb19edd5", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Invalid LLMReplyJsonContent. Skipping.", e);
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                if (!await UpdateLastReplyByAI(backgroundQueryDbModel.ChatId, validJson))
                {
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                await QueueStyleValidatorBackgroundQuery(backgroundQueryDbModel.ChatId);

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("db175ffb-0b32-4aa9-bd0f-c561f8c1d2ba", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }

        private async Task<bool> UpdateLastReplyByAI(string chatId, FindReplaceResultDto findReplaceResultDto)
        {
            if (findReplaceResultDto?.Mutations == null || findReplaceResultDto.Mutations.Length <= 0)
            {
                return true;
            }

            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatId);
            if (hotMessagesDbModel?.Messages == null)
            {
                return false;
            }

            hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.Where(w => w.SourceType == Common.BusinessObjects.MessageSourceType.AI).ToList();
            if (hotMessagesDbModel.Messages.Count <= 0)
            {
                // TODO: if AI hasn't talked in recent messages (hot), well...we could always fetch cold I guess, but that would be highly irregular for roleplay..
                return false;
            }

            IMessageDbModel lastAIMessage = hotMessagesDbModel.Messages.OrderByDescending(o => o.CreatedAtUtc).First();

            if (string.IsNullOrWhiteSpace(lastAIMessage.Content))
            {
                return false;
            }

            IdSentence[] sentences = SentenceSplitter.SplitIntoIdSentences(lastAIMessage.Content);

            if (findReplaceResultDto?.Mutations != null && sentences.Length > 0)
            {
                var byId = sentences.ToDictionary(s => s.Id, StringComparer.OrdinalIgnoreCase);
                var touched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var rejected = new List<string>();
                var quoteChars = new[] { '"', '“', '”' };

                foreach (var mutation in findReplaceResultDto.Mutations)
                {
                    string id = mutation.Id?.Trim() ?? "";

                    if (!byId.TryGetValue(id, out var sentence))
                    {
                        rejected.Add($"{id}: unknown id");
                        continue;
                    }

                    if (sentence.IsLocked)
                    {
                        rejected.Add($"{id}: locked (dialogue)");
                        continue;
                    }

                    if (touched.Contains(id))
                    {
                        rejected.Add($"{id}: already edited");
                        continue;
                    }

                    switch (mutation.Action)
                    {
                        case FindReplaceAction.Delete:
                            sentence.SentenceText = ""; // keep the element so paragraph breaks survive
                            touched.Add(id);
                            break;

                        case FindReplaceAction.Replace:
                            string newText = mutation.Text?.Trim() ?? "";

                            if (newText.Length == 0)
                                rejected.Add($"{id}: empty replacement");
                            else if (newText.Contains('\n'))
                                rejected.Add($"{id}: replacement contains a line break");
                            else if (newText.IndexOfAny(quoteChars) >= 0)
                                rejected.Add($"{id}: replacement contains dialogue");
                            else if (newText.Length > sentence.SentenceText.Length)
                                rejected.Add($"{id}: replacement longer than original");
                            else if (IntroducesNewWords(sentence.SentenceText, newText))
                                rejected.Add($"{id}: introduces new words");
                            else
                            {
                                sentence.SentenceText = newText;
                                touched.Add(id);
                            }
                            break;

                        case FindReplaceAction.Trim:
                        {
                            string cut = mutation.Text?.Trim() ?? "";
                            int idx = cut.Length < 3 ? -1 : sentence.SentenceText.IndexOf(cut, StringComparison.Ordinal);

                            if (idx < 0)
                            {
                                rejected.Add($"{id}: trim text not found in sentence");
                                break;
                            }

                            string result = sentence.SentenceText.Remove(idx, cut.Length);
                            result = Regex.Replace(result, @"\s+", " ");
                            result = Regex.Replace(result, @"\s+([,.;:!?])", "$1").Trim();
                            result = Regex.Replace(result, @"[,;:]+$", "");           // dangling comma at the end
                            if (result.Length > 0 && !".!?…".Contains(result[^1])) result += ".";

                            if (result.Split(' ').Length < 3)
                                rejected.Add($"{id}: trim leaves too little");
                            else
                            {
                                sentence.SentenceText = result;
                                touched.Add(id);
                            }
                            break;
                        }
                        default:
                            rejected.Add($"{id}: undefined action");
                            break;
                    }
                }

                string reassembledText = SentenceSplitter.Reassemble(sentences);

                // never replace the reply with an empty or gutted version.
                if (!string.IsNullOrWhiteSpace(reassembledText)
                    && reassembledText.Length >= lastAIMessage.Content.Length * 0.5)
                {
                    lastAIMessage.Content = reassembledText;
                }

                // log `rejected` somewhere so you can see which prompts produce bad edits
            }

            return await storageService.UpdateHotMessageAsync(chatId, lastAIMessage as MessageDbModel);
        }

        private static readonly Regex WordRx = new(@"[\p{L}']+", RegexOptions.Compiled);

        private static string Norm(string s) => s.Replace('’', '\'').Replace('‘', '\'').ToLowerInvariant();

        public static bool IntroducesNewWords(string original, string replacement, int minLength = 4, int allowed = 0)
        {
            var known = new HashSet<string>(WordRx.Matches(Norm(original)).Select(m => m.Value));
            int fresh = WordRx.Matches(Norm(replacement))
                .Select(m => m.Value)
                .Count(w => w.Length >= minLength && !known.Contains(w));
            return fresh > allowed;
        }

        private async Task QueueStyleValidatorBackgroundQuery(string chatId)
        {
            CreateBackgroundQueryQueryModel queryModel = new()
            {
                ChatId = chatId,
                Priority = BackgroundQueryPriority.Highest,
                LinkedId = null,
                Tags = [BackgroundQuerySystemTags.styleValidator.ToString()],
                DependenciesTags = [],
            };

            await storageService.AddBackgroundQueryAsync(queryModel);
        }
    }
}
