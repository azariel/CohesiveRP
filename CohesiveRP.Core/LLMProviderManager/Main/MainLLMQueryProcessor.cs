using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.LLMProviderManager.Main
{
    public class MainLLMQueryProcessor : ILLMQueryProcessor
    {
        private BackgroundQueryDbModel backgroundQueryDbModel;
        private IPromptContextBuilderFactory contextBuilderFactory;
        private IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
        private IStorageService storageService;
        private IHttpLLMApiProviderService httpLLMApiProviderService;

        public MainLLMQueryProcessor(
            BackgroundQueryDbModel backgroundQueryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService)
        {
            this.backgroundQueryDbModel = backgroundQueryDbModel;
            this.contextBuilderFactory = contextBuilderFactory;
            this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
            this.storageService = storageService;
            this.httpLLMApiProviderService = httpLLMApiProviderService;
        }

        private async Task ProcessMainQueryAsync()
        {
            if (backgroundQueryDbModel.Status == BackgroundQueryStatus.Completed || backgroundQueryDbModel.Status == BackgroundQueryStatus.Error || backgroundQueryDbModel.Status == BackgroundQueryStatus.ProcessedWaitingForFinalInstruction || backgroundQueryDbModel.Status == BackgroundQueryStatus.ProcessingFinalInstruction)
            {
                // The main query was processed, we only need to complete it (very minor, no need to requery the inference server)
                return;
            }

            var promptContext = await BuildContextAsync();

            try
            {
                var globalSettings = await storageService.GetGlobalSettingsAsync();
                var availableLLMApiProviders = globalSettings.LLMProviders.Where(w => w.Tags.Contains(ChatCompletionPresetType.Main)).ToArray();

                if (availableLLMApiProviders == null || availableLLMApiProviders.Length <= 0)
                {
                    LoggingManager.LogToFile("1e48b939-c19d-4097-98fa-bfe8fd857939", $"Couldn't query a LLM Api because no one was configured for [{ChatCompletionPresetType.Main}].");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return;
                }

                IHttpLLMApiQueryResponseDto response = await httpLLMApiProviderService.QueryApiAsync(ChatCompletionPresetType.Main.ToString(), availableLLMApiProviders, promptContext);

                // TODO: update background query here
                var responseMessages = response.Messages;

                backgroundQueryDbModel.Content = JsonCommonSerializer.SerializeToString(response.Messages);
                backgroundQueryDbModel.Status = BackgroundQueryStatus.ProcessingFinalInstruction;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("52cac4f0-cbfd-4786-9530-c96b6d909515", $"The query to HttpLLMApiProviderService failed. Unhandled error.", e);
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                return;
            }
        }

        private async Task<IPromptContext> BuildContextAsync()
        {
            // Generate a context builder appropriate for our MainQuery
            IPromptContextBuilder contextBuilder = await contextBuilderFactory.GenerateAsync(BackgroundQuerySystemTags.main, promptContextElementBuilderFactory, storageService);
            return await contextBuilder.BuildAsync(backgroundQueryDbModel.ChatId);
        }

        public async Task<bool> QueueProcessAsync()
        {
            if (backgroundQueryDbModel == null)
            {
                return false;
            }

            _ = Task.Run(async () => await ProcessMainQueryAsync());

            return true;
        }

        public async Task<BackgroundQueryDbModel> GetBackgroundQueryDbModelAsync() => backgroundQueryDbModel;

        /// <summary>
        /// Process the resulting completed query. If it was a 'main', it'll add a new AI message, if it was a sceneTracker, it'll attach the tracker, if it was a summary, it'll attach the summary to an existing message, etc.
        /// </summary>
        public async Task ProcessCompletedQueryAsync()
        {
            if (backgroundQueryDbModel == null || backgroundQueryDbModel.Status != BackgroundQueryStatus.ProcessingFinalInstruction)
            {
                LoggingManager.LogToFile("12498826-8f44-4f5f-ac9f-51f7de6e08fa", $"Ignoring completed background query [{backgroundQueryDbModel?.BackgroundQueryId}]. Status was [{backgroundQueryDbModel?.Status}].");
                return;
            }

            if (string.IsNullOrWhiteSpace(backgroundQueryDbModel.Content))
            {
                LoggingManager.LogToFile("a44dbd75-61fb-46fc-98df-dcea7eaa83c6", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. The Content was null or empty. Task will be set to Pending status for re-generation.");
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                return;
            }

            // Deserialize the generic content into a list of messages
            try
            {
                LLMApiResponseMessage[] messages = JsonCommonSerializer.DeserializeFromString<LLMApiResponseMessage[]>(backgroundQueryDbModel.Content);

                foreach (var message in messages)
                {
                    //string deserializedMessage = message.Content;

                    //try
                    //{
                    //    deserializedMessage = JsonCommonSerializer.DeserializeFromString<string>(message.Content);
                    //} catch (Exception) { }

                    // Add the AI reply message to the end of the chat
                    CreateMessageQueryModel messageQueryModel = new()
                    {
                        ChatId = backgroundQueryDbModel.ChatId,
                        SourceType = MessageSourceType.AI,
                        MessageContent = ChatMessageParserUtils.ParseMessage(message.Content),
                        CreatedAtUtc = DateTime.UtcNow,
                    };

                    var newMessageInStorage = await storageService.CreateMessageAsync(messageQueryModel);
                    backgroundQueryDbModel.LinkedMessageId = newMessageInStorage.MessageId;
                }

                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("3323ca32-a0b4-414f-a0a7-eedea88c4099", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
            }
        }
    }
}
