using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.LLMProviderProcessors.SceneAnalyzer.BusinessObjects;
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

namespace CohesiveRP.Core.LLMProviderProcessors.SceneAnalyzer
{
    public class SceneAnalyzerLLMQueryProcessor : LLMQueryProcessor
    {
        public SceneAnalyzerLLMQueryProcessor(
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
                return false;
            }

            // Deserialize the generic content into a valid Scene Analyzer
            try
            {
                LLMApiResponseMessage LLMmessage = messages.LastOrDefault();
                IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextSceneAnalyzerInstrBuilder);
                if (shareableContextLink == null)
                {
                    LoggingManager.LogToFile("e214f6b2-ffda-4fd8-8b92-d3b0b565d297", $"No ShareableContextLink of type [{nameof(PromptContextSceneAnalyzerInstrBuilder)} found.]");
                    return false;
                }

                // Parse the response as Json
                SceneAnalyzerLLMResponseDto responseDto = null;
                try
                {
                    responseDto = LLMResponseParser.ParseFromApiMessageContent<SceneAnalyzerLLMResponseDto>(backgroundQueryDbModel.Content);
                } catch (Exception ex)
                {
                    LoggingManager.LogToFile("a604227f-e724-4807-92e9-76424d86b9a3", $"The LLM response was malformed [{backgroundQueryDbModel.Content}].");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;// drop
                    return false;
                }

                if(responseDto?.PlayerAnalyze == null || responseDto?.CharactersAnalyze == null || responseDto?.SceneCategory == null)
                {
                    LoggingManager.LogToFile("82ffbc70-cef7-4fc7-9183-c64b9bced657", $"The LLM response was malformed [{backgroundQueryDbModel.Content}].");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;// drop
                    return false;
                }

                // Replace the scene analyzer tied to this chat with the new one
                SceneAnalyzerDbModel dbModel = new()
                {
                    ChatId = backgroundQueryDbModel.ChatId,
                    CharactersAnalyze = responseDto.CharactersAnalyze,
                    PlayerAnalyze = responseDto.PlayerAnalyze,
                    SceneCategory = responseDto.SceneCategory,
                    LinkedMessageId = shareableContextLink.Value as string,
                };

                SceneAnalyzerDbModel updatedMessageInStorage = await storageService.CreateOrUpdateSceneAnalyzerAsync(dbModel);
                if (updatedMessageInStorage == null)
                {
                    LoggingManager.LogToFile("1cf1c697-34b3-438c-8e24-547d29867fdb", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't update storage. Skipping.");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                // Prebuild the images to show in the UI according to context

                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("d697e65a-7997-44e0-8fc5-d40355222627", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
            }

            return true;
        }
    }
}
