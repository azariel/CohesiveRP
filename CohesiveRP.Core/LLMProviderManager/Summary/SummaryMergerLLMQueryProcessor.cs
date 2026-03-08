using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Summary;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.LLMProviderManager.Main
{
    public class SummaryMergerLLMQueryProcessor : LLMQueryProcessor
    {
        public SummaryMergerLLMQueryProcessor(
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

        /// <summary>
        /// Process the resulting completed query. If it was a 'main', it'll add a new AI message, if it was a sceneTracker, it'll attach the tracker, if it was a summary, it'll attach the summary to an existing message, etc.
        /// </summary>
        public override async Task ProcessCompletedQueryAsync()
        {
            if (backgroundQueryDbModel == null || backgroundQueryDbModel.Status != BackgroundQueryStatus.ProcessingFinalInstruction)
            {
                LoggingManager.LogToFile("16a9d632-d4e7-439a-b6a1-52173ea370f1", $"Ignoring background query [{backgroundQueryDbModel?.BackgroundQueryId}]. Status was [{backgroundQueryDbModel?.Status}].");
                return;
            }

            if (string.IsNullOrWhiteSpace(backgroundQueryDbModel.Content))
            {
                LoggingManager.LogToFile("4c2f3f74-e0b3-49a4-8b20-296e91724787", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content was null or empty. Task will be set to Pending status for re-generation.");
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                return;
            }

            // Deserialize the generic content into a list of messages
            try
            {
                LLMApiResponseMessage[] messages = JsonCommonSerializer.DeserializeFromString<LLMApiResponseMessage[]>(backgroundQueryDbModel.Content);

                if (messages.Length != 1)
                {
                    LoggingManager.LogToFile("215dc539-06ba-4b1b-9850-b3d49a8b44c4", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content embedding [{messages.Length}] messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                }

                if (!await ExecuteCompletingProcess())
                    return;

                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("e6db3317-7952-4ba9-900c-39d97ec510d3", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
            }
        }

        private async Task<bool> ExecuteCompletingProcess()
        {
            GlobalSettingsDbModel settings = await storageService.GetGlobalSettingsAsync();
            SummaryDbModel summaryDbModel = await storageService.GetSummaryAsync(backgroundQueryDbModel.ChatId);
            List<SummaryEntryDbModel> summariesToParse = null;
            int summarizeLastXTokens = 0;

            switch (tag)
            {
                case BackgroundQuerySystemTags.shortSummary:
                    summariesToParse = summaryDbModel.ShortTermSummaries;
                    summarizeLastXTokens = settings.Summary.Medium.SummarizeLastXTokens;
                    break;
                case BackgroundQuerySystemTags.mediumSummary:
                    summariesToParse = summaryDbModel.ShortTermSummaries;
                    summarizeLastXTokens = settings.Summary.Long.SummarizeLastXTokens;
                    break;
                case BackgroundQuerySystemTags.longSummary:
                    summariesToParse = summaryDbModel.MediumTermSummaries;
                    summarizeLastXTokens = settings.Summary.Extra.SummarizeLastXTokens;
                    break;
                case BackgroundQuerySystemTags.extraSummary:
                    summariesToParse = summaryDbModel.LongTermSummaries;
                    summarizeLastXTokens = settings.Summary.Overflow.SummarizeLastXTokens;
                    break;
                default:
                    throw new Exception($"Unhandled tag [{tag}] in .");
            }

            summariesToParse ??= new();

            List<SummaryEntryDbModel> lowerTermSummariesToSummarize = new();
            int tokensCounter = 0;
            var orderedSummariesToParse = summariesToParse.OrderBy(o => o.CreatedAtUtc);
            foreach (SummaryEntryDbModel lowerTermSummary in orderedSummariesToParse)
            {
                lowerTermSummariesToSummarize.Add(lowerTermSummary);
                tokensCounter += TokensUtils.Count(lowerTermSummary.Content);

                if (tokensCounter >= summarizeLastXTokens)
                {
                    break;
                }
            }

            // You can't summarize a single chunk, so despite what the config says, we'll do at least 2
            if (lowerTermSummariesToSummarize.Count < 2 && summariesToParse.Count > 1)
            {
                lowerTermSummariesToSummarize = orderedSummariesToParse.Take(2).ToList();
            }

            // We now need to remove the summaries that were merged and add the merged summary to the next tier
            // Add new summary to higher tier
            LLMApiResponseMessage[] messages = JsonCommonSerializer.DeserializeFromString<LLMApiResponseMessage[]>(backgroundQueryDbModel.Content);

            if (messages.Length != 1)
            {
                LoggingManager.LogToFile("aa1dc4b6-635f-4d42-91c8-b525c16b0684", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content embedding [{messages.Length}] messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                return false;
            }

            CreateSummaryQueryModel messageQueryModel = new()
            {
                ChatId = backgroundQueryDbModel.ChatId,
                MessageIdTracker = backgroundQueryDbModel.LinkedId,
                Content = ChatMessageParserUtils.ParseMessage(messages[0].Content),
                CreatedAtUtc = DateTime.UtcNow,
            };

            switch (tag)
            {
                case BackgroundQuerySystemTags.mediumSummary:
                    if(await storageService.AddMediumTermSummaryAsync(messageQueryModel) == null)
                        return false;

                    if (!await storageService.DeleteShortTermSummariesEntriesAsync(backgroundQueryDbModel.ChatId, [..lowerTermSummariesToSummarize.Select(s => s.SummaryEntryId)]))
                        return false;
                    break;
                case BackgroundQuerySystemTags.longSummary:
                    if(await storageService.AddLongTermSummaryAsync(messageQueryModel) == null)
                        return false;

                    if (!await storageService.DeleteMediumTermSummariesEntriesAsync(backgroundQueryDbModel.ChatId, [..lowerTermSummariesToSummarize.Select(s => s.SummaryEntryId)]))
                        return false;
                    break;
                case BackgroundQuerySystemTags.extraSummary:
                    if(await storageService.AddExtraTermSummaryAsync(messageQueryModel) == null)
                        return false;

                    if (!await storageService.DeleteLongTermSummariesEntriesAsync(backgroundQueryDbModel.ChatId, [..lowerTermSummariesToSummarize.Select(s => s.SummaryEntryId)]))
                        return false;
                    break;
                default:
                    throw new Exception($"Unhandled tag [{tag}] in .");
            }

            return true;
        }
    }
}
