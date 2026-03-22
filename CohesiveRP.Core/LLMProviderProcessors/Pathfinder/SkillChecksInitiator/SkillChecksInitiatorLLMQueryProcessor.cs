using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator
{
    public class SkillChecksInitiatorLLMQueryProcessor : LLMQueryProcessor
    {
        public SkillChecksInitiatorLLMQueryProcessor(
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

        public override async Task ProcessCompletedQueryAsync()
        {
            if (backgroundQueryDbModel == null || backgroundQueryDbModel.Status != BackgroundQueryStatus.ProcessingFinalInstruction)
            {
                LoggingManager.LogToFile("60a3768c-20d8-400f-924b-1a8f90a9ddd5", $"Ignoring background query [{backgroundQueryDbModel?.BackgroundQueryId}]. Status was [{backgroundQueryDbModel?.Status}].");
                return;
            }

            if (string.IsNullOrWhiteSpace(backgroundQueryDbModel.Content))
            {
                LoggingManager.LogToFile("e1dbc0cd-7c79-4d07-b2c0-70243fca5297", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content was null or empty. Task will be set to Pending status for re-generation.");
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                return;
            }

            // Deserialize the generic content into a valid SkillsChecksInitiator
            try
            {
                LLMApiResponseMessage LLMmessage = JsonCommonSerializer.DeserializeFromString<LLMApiResponseMessage[]>(backgroundQueryDbModel.Content).LastOrDefault();

                if (LLMmessage == null)
                {
                    LoggingManager.LogToFile("eecfd8f9-23cc-487c-9d98-3a86eacbd4cf", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content no messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    return;
                }

                // TODO: Actually call the backend to DO the skill checks, compute them and find a way to ADD them to the MAIN backgroundQuery


                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("025c0b1d-cfef-4a20-bab7-8bde505060be", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
            }
        }
    }
}
