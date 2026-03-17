using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.PromptContext;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.LLMProviderManager.Main
{
    public class ShortSummaryLLMQueryProcessor : LLMQueryProcessor
    {
        public ShortSummaryLLMQueryProcessor(
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
                LoggingManager.LogToFile("1d7442145-1b29-499c-963a-64264155fb3f", $"Ignoring background query [{backgroundQueryDbModel?.BackgroundQueryId}]. Status was [{backgroundQueryDbModel?.Status}].");
                return;
            }

            if (string.IsNullOrWhiteSpace(backgroundQueryDbModel.Content))
            {
                LoggingManager.LogToFile("fc9174e2-55ca-40e9-b8fc-6870e111a069", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content was null or empty. Task will be set to Pending status for re-generation.");
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
                    LoggingManager.LogToFile("8f7def6e-a58b-4db3-9db0-9ea3ac25d6fa", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content embedding [{messages.Length}] messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    return;
                }

                // Add the new summary entry
                CreateSummaryQueryModel messageQueryModel = new()
                {
                    ChatId = backgroundQueryDbModel.ChatId,
                    MessageIdTracker = backgroundQueryDbModel.LinkedId,
                    Content = ChatMessageParserUtils.ParseMessage(messages[0].Content),
                    CreatedAtUtc = DateTime.UtcNow,
                };

                ISummaryEntryDbModel newSummaryEntryInStorage = await storageService.AddShortTermSummaryAsync(messageQueryModel);
                GlobalSettingsDbModel globalSettings = await storageService.GetGlobalSettingsAsync();

                // Update the summarized messages in db
                if(!await UpdateSummarizedMessagesAsync(messageQueryModel.ChatId))
                {
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return;
                }

                //backgroundQueryDbModel.LinkedId = newSummaryEntryInStorage.MessageIdTracker;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("27b69224-425c-4df2-8ea9-9c121b689a13", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
            }
        }

        private async Task<bool> UpdateSummarizedMessagesAsync(string chatId)
        {
            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatId);

            hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.OrderByDescending(o => o.CreatedAtUtc).ToList();
            string[] messageIdsProcessedAgainstLLM = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextLastXMessagesToSummarizeBuilder)?.Value as string[];
            if (messageIdsProcessedAgainstLLM == null)
            {
                LoggingManager.LogToFile("6653a400-18f7-4d98-a0ed-b8959e4049f6", $"No ShareableContextLink of type [{nameof(PromptContextLastXMessagesToSummarizeBuilder)} found.]");
                return false;
            }

            // Process the messages
            hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.OrderBy(o => o.CreatedAtUtc).ToList();

            foreach (var message in messageIdsProcessedAgainstLLM)
            {
                hotMessagesDbModel.Messages.FirstOrDefault(w => w.MessageId == message)?.Summarized = true;
            }

            HotMessagesDbModel request = new HotMessagesDbModel
            {
                ChatId = chatId,
                Messages = hotMessagesDbModel.Messages.Cast<MessageDbModel>().ToList(),
            };

            await storageService.UpdateHotMessagesAsync(request);
            return true;
        }
    }
}
