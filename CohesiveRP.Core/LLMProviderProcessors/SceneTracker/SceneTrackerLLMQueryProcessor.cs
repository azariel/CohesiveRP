using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects.Visual;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.SceneTracker
{
    public class SceneTrackerLLMQueryProcessor : LLMQueryProcessor
    {
        public SceneTrackerLLMQueryProcessor(
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
                LLMApiResponseMessage LLMmessage = messages.LastOrDefault();
                IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextSceneTrackerInstrBuilder);
                if (shareableContextLink == null)
                {
                    LoggingManager.LogToFile("c1a42696-c67e-484f-86b9-e0a41f501221", $"No ShareableContextLink of type [{nameof(PromptContextSceneTrackerInstrBuilder)} found.]");
                    return false;
                }

                string sceneTrackerJson = LLMResponseParser.ParseOnlyJson(messages.First().Content);

                // validate that the sceneTrackerJson is valid json

                try
                {
                    JsonCommonSerializer.DeserializeFromString<VisualSceneTracker>(sceneTrackerJson);
                } catch (Exception e)
                {
                    LoggingManager.LogToFile("9ced233b-249d-456f-a2bf-48e847daa024", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Invalid sceneTrackerJson. Skipping.", e);
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                VisualSceneTracker validJson = JsonCommonSerializer.DeserializeFromString<VisualSceneTracker>(sceneTrackerJson);

                string linkedMessageId = shareableContextLink.Value as string;
                SceneTrackerDbModel queryModel = new()
                {
                    ChatId = backgroundQueryDbModel.ChatId,
                    LinkMessageId = linkedMessageId,
                    Content = sceneTrackerJson,
                };

                SceneTrackerDbModel sceneTrackerDbModel = await storageService.CreateOrUpdateSceneTrackerAsync(queryModel);
                if (sceneTrackerDbModel == null)
                {
                    LoggingManager.LogToFile("c352fa3d-7019-4ed1-923a-d4b17db6d7a1", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't update storage. Skipping.");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                // Queue the sceneTracker validator
                await QueueSceneTrackerValidatorBackgroundQuery(backgroundQueryDbModel.ChatId, backgroundQueryDbModel.LinkedId);

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("be1a2097-a9d4-4242-9a9d-4e60429f59df", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }

        private async Task QueueSceneTrackerValidatorBackgroundQuery(string chatId, string linkedId)
        {
            CreateBackgroundQueryQueryModel queryModel = new()
            {
                ChatId = chatId,
                Priority = BackgroundQueryPriority.Highest,
                LinkedId = linkedId,
                Tags = [BackgroundQuerySystemTags.sceneTrackerValidator.ToString()],
                DependenciesTags = [],// at this point in time, we don't depend on anything, we're the most important query
            };

            await storageService.AddBackgroundQueryAsync(queryModel);
        }
    }
}