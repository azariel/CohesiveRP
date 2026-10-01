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
using CohesiveRP.Storage.DataAccessLayer.Cohesion.StyleCohesion.BusinessObjects;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.Cohesion
{
    public class StyleValidatorLLMQueryProcessor : LLMQueryProcessor
    {
        public StyleValidatorLLMQueryProcessor(
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
                string StyleValidationJson = LLMResponseParser.ParseOnlyJson(messages.First().Content);
                StyleValidationResult result = null;

                try
                {
                    result = JsonCommonSerializer.DeserializeFromString<StyleValidationResult>(StyleValidationJson);
                    result ??= new();
                } catch (Exception e)
                {
                    LoggingManager.LogToFile("9d0358a6-2d60-4391-b608-ff4ffa0ad4c9", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Invalid StyleValidation Json format. Task will be reprocessed from start.", e);
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                var linkedStylesInStorage = await storageService.GetStyleCohesionsAsync(s => s.ChatId == backgroundQueryDbModel.ChatId);
                if (linkedStylesInStorage == null)
                {
                    LoggingManager.LogToFile("98f2994e-a477-47c3-9780-1de09f35bda1", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. There was NO sceneTracker for this chat. The validation is therefore useless.");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                StyleCohesionDbModel linkedStyleInStorage = null;
                if (linkedStylesInStorage.Length <= 0)
                {
                    linkedStyleInStorage = await storageService.AddStyleCohesionAsync(new StyleCohesionDbModel
                    {
                        ChatId = backgroundQueryDbModel.ChatId,
                        Content = new StyleCohesionElement(),
                    });
                } else
                {
                    linkedStyleInStorage = linkedStylesInStorage.First();
                }

                if (result.Recommendations.Length <= 0)
                {
                    // The validator didn't find any recommendations, the current Style is alright
                    // We're done! the message from the AI is correct and final!
                } else
                {
                    // Start by updating the StyleCohesion to add the suggestions
                    linkedStyleInStorage.Content = new StyleCohesionElement
                    {
                        Content = JsonCommonSerializer.SerializeToString(result),
                    };

                    await storageService.UpdateStyleCohesionAsync(linkedStyleInStorage);
                    await QueueStyleEditorBackgroundQuery(backgroundQueryDbModel.ChatId);
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("ade6b34d-9f35-4ac8-aa32-8833330dcfeb", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }

        private async Task QueueStyleEditorBackgroundQuery(string chatId)
        {
            CreateBackgroundQueryQueryModel queryModel = new()
            {
                ChatId = chatId,
                Priority = BackgroundQueryPriority.Highest,
                LinkedId = null,
                Tags = [BackgroundQuerySystemTags.styleEdition.ToString()],
                DependenciesTags = [],
            };

            await storageService.AddBackgroundQueryAsync(queryModel);
        }
    }
}
