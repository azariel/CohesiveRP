using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeDirection;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeDirection.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.ChatAdditions
{
    public class NarrativeDirectionLLMQueryProcessor : LLMQueryProcessor
    {
        public NarrativeDirectionLLMQueryProcessor(
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

                // Replace the NarrativeDirection tied to this chat with the new one
                var currentDbModels = await storageService.GetNarrativeDirectionsAsync(s => s.ChatId == backgroundQueryDbModel.ChatId);
                var currentDbModel = currentDbModels?.FirstOrDefault();
                if (currentDbModel == null)
                {
                    // Create a brand new one
                    currentDbModel = new NarrativeDirectionDbModel
                    {
                        ChatId = backgroundQueryDbModel.ChatId,
                        Content = new NarrativeDirectionElement
                        {
                            Content = LLMMessageResult,
                        },
                    };

                    await storageService.AddNarrativeDirectionAsync(currentDbModel);
                } else
                {
                    currentDbModel.Content = new NarrativeDirectionElement
                    {
                        Content = LLMMessageResult,
                    };

                    await storageService.UpdateNarrativeDirectionAsync(currentDbModel);
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("156e41c5-cda7-4d40-90ba-c137c8093b48", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }
    }
}
