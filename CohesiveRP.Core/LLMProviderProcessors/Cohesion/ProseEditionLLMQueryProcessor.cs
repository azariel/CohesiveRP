using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.ProseCohesion.BusinessObjects;
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
                if (!await UpdateLastReplyByAI(backgroundQueryDbModel.ChatId, LLMMessageResult))
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

        private async Task<bool> UpdateLastReplyByAI(string chatId, string newMessageContent)
        {
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

            lastAIMessage.Content = newMessageContent;
            return await storageService.UpdateHotMessageAsync(chatId, lastAIMessage as MessageDbModel);
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
