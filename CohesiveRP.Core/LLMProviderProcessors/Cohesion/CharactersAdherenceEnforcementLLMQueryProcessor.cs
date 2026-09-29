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
using CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement.BusinessObjects;
using CohesiveRP.Storage.DTOs.CohesionEnforcement;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.Cohesion
{
    public class CharactersAdherenceEnforcementLLMQueryProcessor : LLMQueryProcessor
    {
        public CharactersAdherenceEnforcementLLMQueryProcessor(
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
                string LLMMessageResult = LLMResponseParser.ParseOnlyJson(messages.First().Content);

                // deserialize the CohesionEnforcement
                CharactersCohesionEnforcementResult cohesionEnforcement = null;

                try
                {
                    cohesionEnforcement = JsonCommonSerializer.DeserializeFromString<CharactersCohesionEnforcementResult>(LLMMessageResult);
                } catch (Exception e)
                {
                    LoggingManager.LogToFile("a81acc34-e4b3-4147-a5b5-2c729d181fad", $"Failed to deserialize CharactersCohesionEnforcementResult from LLM response.", e);
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                var finalContent = JsonCommonSerializer.SerializeToString(cohesionEnforcement);

                // Replace the CohesionEnforcement tied to this chat with the new one
                var currentDbModels = await storageService.GetCharactersCohesionEnforcementsAsync(s => s.ChatId == backgroundQueryDbModel.ChatId);
                var currentDbModel = currentDbModels?.FirstOrDefault();
                if (currentDbModel == null)
                {
                    // Create a brand new one
                    currentDbModel = new CharactersCohesionEnforcementDbModel
                    {
                        ChatId = backgroundQueryDbModel.ChatId,
                        Content = new CharactersCohesionEnforcementElement
                        {
                            Content = finalContent,
                        },
                    };

                    await storageService.AddCharactersCohesionEnforcementAsync(currentDbModel);
                } else
                {
                    currentDbModel.Content = new CharactersCohesionEnforcementElement
                    {
                        Content = finalContent,
                    };

                    await storageService.UpdateCharactersCohesionEnforcementAsync(currentDbModel);
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("ba0ccff5-42a5-4c78-a83f-a25bab7ace20", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }
    }
}
