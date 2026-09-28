using CohesiveRP.Common.BusinessObjects;
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
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.SceneTracker
{
    public class SceneTrackerValidatorLLMQueryProcessor : LLMQueryProcessor
    {
        ISceneTrackerPostProcess sceneTrackerPostProcess;

        public SceneTrackerValidatorLLMQueryProcessor(
            ChatCompletionPresetType completionPresetType,
            BackgroundQuerySystemTags tag,
            BackgroundQueryDbModel backgroundQueryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService,
            ISummaryService summaryService,
            ISceneTrackerPostProcess sceneTrackerPostProcess) : base(
                completionPresetType,
                tag,
                backgroundQueryDbModel,
                contextBuilderFactory,
                promptContextElementBuilderFactory,
                storageService,
                httpLLMApiProviderService,
                summaryService)
        {
            this.sceneTrackerPostProcess = sceneTrackerPostProcess;
        }

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
                    LoggingManager.LogToFile("03b00f6f-fe13-46c9-8a3b-59f3a7331b8e", $"No ShareableContextLink of type [{nameof(PromptContextSceneTrackerInstrBuilder)} found.]");
                    return false;
                }

                // This contains the validation result. It may contains suggestions on things to modify or no suggestions at all depending on the previously generated sceneTracker
                var AImessage = messages.First().Content;
                string sceneTrackerValidationJson = LLMResponseParser.ParseOnlyJson(AImessage);

                // The goal of the game here is to aim for no suggestion at all AKA the previous step generating a proper SceneTracker that is coherent and consistent with the current scene
                // But, if the validator raise any issues, we need to queue another background query to tackle it and refine the sceneTracker before processing to the next step
                SceneTrackerValidationResult result = null;
                try
                {
                    result = JsonCommonSerializer.DeserializeFromString<SceneTrackerValidationResult>(sceneTrackerValidationJson);
                    result ??= new();
                } catch (Exception e)
                {
                    LoggingManager.LogToFile("5cadbea9-76dc-4fd2-a5c6-3c89ad21a89a", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Invalid sceneTrackerValidation Json format. Task will be reprocessed from start.", e);
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                var currentSceneTrackerInStorage = await storageService.GetSceneTrackerAsync(backgroundQueryDbModel.ChatId);
                if (currentSceneTrackerInStorage == null)
                {
                    LoggingManager.LogToFile("619646fc-849b-47d7-8be2-88e000a88af9", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. There was NO sceneTracker for this chat. The validation is therefore useless.");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                if (result.Recommendations.Length <= 0)
                {
                    // The validator didn't find any recommendations, the sceneTracker that we have in DB is ok
                    await sceneTrackerPostProcess.Process(completionPresetType, tag, backgroundQueryDbModel, shareableContextLink, currentSceneTrackerInStorage.Content);
                } else
                {
                    // Start by updating the sceneTracker to add the suggestions
                    currentSceneTrackerInStorage.Suggestions = string.Join($"{(result.Recommendations.Length > 0? "- " : string.Empty)}{Environment.NewLine}- ", result.Recommendations);
                    await storageService.CreateOrUpdateSceneTrackerAsync(currentSceneTrackerInStorage);
                    await QueueSceneTrackerRefinerBackgroundQuery(backgroundQueryDbModel.ChatId);
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("e606933e-790c-482e-8038-94386488403a", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }

        private async Task QueueSceneTrackerRefinerBackgroundQuery(string chatId)
        {
            CreateBackgroundQueryQueryModel queryModel = new()
            {
                ChatId = chatId,
                Priority = BackgroundQueryPriority.Highest,
                LinkedId = null,
                Tags = [BackgroundQuerySystemTags.sceneTrackerRefiner.ToString()],
                DependenciesTags = [
                    // logical, but commented so that the worker doesn't filter out that backgroundQuery (concurrency)
                    //BackgroundQuerySystemTags.sceneTracker.ToString(),
                    //BackgroundQuerySystemTags.sceneTrackerValidator.ToString(),

                    BackgroundQuerySystemTags.skillChecksInitiator.ToString(),
                ],
            };

            await storageService.AddBackgroundQueryAsync(queryModel);
        }
    }
}