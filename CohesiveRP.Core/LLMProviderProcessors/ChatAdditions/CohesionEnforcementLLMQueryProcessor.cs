using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.CohesionEnforcement;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.CohesionEnforcement.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.ChatAdditions
{
    public class CohesionEnforcementLLMQueryProcessor : LLMQueryProcessor
    {
        public CohesionEnforcementLLMQueryProcessor(
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

                // Replace the CohesionEnforcement tied to this chat with the new one
                var currentDbModels = await storageService.GetCohesionEnforcementsAsync(s => s.ChatId == backgroundQueryDbModel.ChatId);
                var currentDbModel = currentDbModels?.FirstOrDefault();
                if (currentDbModel == null)
                {
                    // Create a brand new one
                    currentDbModel = new CohesionEnforcementDbModel
                    {
                        ChatId = backgroundQueryDbModel.ChatId,
                        Content = new CohesionEnforcementElement
                        {
                            Content = LLMMessageResult,
                        },
                    };

                    await storageService.AddCohesionEnforcementAsync(currentDbModel);
                } else
                {
                    currentDbModel.Content = new CohesionEnforcementElement
                    {
                        Content = LLMMessageResult,
                    };

                    await storageService.UpdateCohesionEnforcementAsync(currentDbModel);
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("6068df55-bd22-424f-a4db-b1be7d450eed", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }
    }
}
