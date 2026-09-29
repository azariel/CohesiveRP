using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.ProseCohesion.BusinessObjects;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.Cohesion
{
    public class ProseValidatorLLMQueryProcessor : LLMQueryProcessor
    {
        public ProseValidatorLLMQueryProcessor(
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
                string proseValidationJson = LLMResponseParser.ParseOnlyJson(messages.First().Content);
                ProseValidationResult result = null;

                try
                {
                    result = JsonCommonSerializer.DeserializeFromString<ProseValidationResult>(proseValidationJson);
                    result ??= new();
                } catch (Exception e)
                {
                    LoggingManager.LogToFile("3f58203f-cce5-4d9c-b9fb-3952f3fe0d76", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Invalid ProseValidation Json format. Task will be reprocessed from start.", e);
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                var linkedProsesInStorage = await storageService.GetProseCohesionsAsync(s => s.ChatId == backgroundQueryDbModel.ChatId);
                if (linkedProsesInStorage == null)
                {
                    LoggingManager.LogToFile("619646fc-849b-47d7-8be2-88e000a88af9", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. There was NO sceneTracker for this chat. The validation is therefore useless.");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                ProseCohesionDbModel linkedProseInStorage = null;
                if (linkedProsesInStorage.Length <= 0)
                {
                    linkedProseInStorage = await storageService.AddProseCohesionAsync(new ProseCohesionDbModel
                    {
                        ChatId = backgroundQueryDbModel.ChatId,
                        Content = new ProseCohesionElement(),
                    });
                } else
                {
                    linkedProseInStorage = linkedProsesInStorage.First();
                }

                if (result.Recommendations.Length <= 0)
                {
                    // The validator didn't find any recommendations, the current prose is alright
                    await QueueStyleValidatorBackgroundQuery(backgroundQueryDbModel.ChatId);
                } else
                {
                    // Start by updating the proseCohesion to add the suggestions
                    linkedProseInStorage.Content = new ProseCohesionElement
                    {
                        Content = string.Join($"{(result.Recommendations.Length > 0 ? "- " : string.Empty)}{Environment.NewLine}- ", result.Recommendations),
                    };

                    await storageService.UpdateProseCohesionAsync(linkedProseInStorage);
                    await QueueProseEditorBackgroundQuery(backgroundQueryDbModel.ChatId);
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("c25ebf57-2765-44ec-b12f-3c326249a821", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }

        private async Task QueueProseEditorBackgroundQuery(string chatId)
        {
            CreateBackgroundQueryQueryModel queryModel = new()
            {
                ChatId = chatId,
                Priority = BackgroundQueryPriority.Highest,
                LinkedId = null,
                Tags = [BackgroundQuerySystemTags.proseEdition.ToString()],
                DependenciesTags = [],
            };

            await storageService.AddBackgroundQueryAsync(queryModel);
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
