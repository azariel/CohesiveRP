using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
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
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.SceneTracker;

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

        public override async Task ProcessCompletedQueryAsync()
        {
            if (backgroundQueryDbModel == null || backgroundQueryDbModel.Status != BackgroundQueryStatus.ProcessingFinalInstruction)
            {
                LoggingManager.LogToFile("cba14f7f-5e09-4d11-9aca-083c7abd2473", $"Ignoring background query [{backgroundQueryDbModel?.BackgroundQueryId}]. Status was [{backgroundQueryDbModel?.Status}].");
                return;
            }

            if (string.IsNullOrWhiteSpace(backgroundQueryDbModel.Content))
            {
                LoggingManager.LogToFile("96bcbd1a-41fc-4035-8e8e-44f47d5b2e0e", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content was null or empty. Task will be set to Pending status for re-generation.");
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                return;
            }

            // Deserialize the generic content into a valid Scene Tracker
            try
            {
                LLMApiResponseMessage LLMmessage = JsonCommonSerializer.DeserializeFromString<LLMApiResponseMessage[]>(backgroundQueryDbModel.Content).LastOrDefault();

                if (LLMmessage == null)
                {
                    LoggingManager.LogToFile("a86f6951-e51a-44c2-ba9d-71433b0d9407", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content no messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    return;
                }

                IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextSceneTrackerInstrBuilder);
                if (shareableContextLink == null)
                {
                    LoggingManager.LogToFile("c1a42696-c67e-484f-86b9-e0a41f501221", $"No ShareableContextLink of type [{nameof(PromptContextSceneTrackerInstrBuilder)} found.]");
                    return;
                }

                // Replace the scene tracker tied to this chat with the new one
                CreateSceneTrackerQueryModel queryModel = new()
                {
                    ChatId = backgroundQueryDbModel.ChatId,
                    LinkMessageId = shareableContextLink.Value as string,
                    Content = LLMmessage.Content,
                };

                SceneTrackerDbModel updatedMessageInStorage = await storageService.UpdateSceneTrackerAsync(queryModel);
                if(updatedMessageInStorage == null)
                { 
                    LoggingManager.LogToFile("c352fa3d-7019-4ed1-923a-d4b17db6d7a1", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't update storage. Skipping.");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return;
                }

                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("be1a2097-a9d4-4242-9a9d-4e60429f59df", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
            }
        }
    }
}
