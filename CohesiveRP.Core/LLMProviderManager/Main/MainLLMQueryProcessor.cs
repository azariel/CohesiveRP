using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.LLMProviderManager.Main
{
    public class MainLLMQueryProcessor : ILLMQueryProcessor
    {
        private BackgroundQueryDbModel queryDbModel;
        private IPromptContextBuilderFactory contextBuilderFactory;
        private IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
        private IStorageService storageService;
        private IHttpLLMApiProviderService httpLLMApiProviderService;

        public MainLLMQueryProcessor(
            BackgroundQueryDbModel queryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService)
        {
            this.queryDbModel = queryDbModel;
            this.contextBuilderFactory = contextBuilderFactory;
            this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
            this.storageService = storageService;
            this.httpLLMApiProviderService = httpLLMApiProviderService;
        }

        private async Task ProcessMainQueryAsync()
        {
            var promptContext = await BuildContextAsync();

            try
            {
                var globalSettings = await storageService.GetGlobalSettingsAsync();
                var availableLLMApiProviders = globalSettings.LLMProviders.Where(w => w.Tags.Contains(ChatCompletionPresetType.Main)).ToArray();

                if (availableLLMApiProviders == null || availableLLMApiProviders.Length <= 0)
                {
                    LoggingManager.LogToFile("1e48b939-c19d-4097-98fa-bfe8fd857939", $"Couldn't query a LLM Api because no one was configured for [{ChatCompletionPresetType.Main}].");
                    queryDbModel.Status = BackgroundQueryStatus.Error;
                    return;
                }

                HttpLLMApiProviderQueryResponseDto response = await httpLLMApiProviderService.QueryApiAsync(ChatCompletionPresetType.Main.ToString(), availableLLMApiProviders, promptContext);
                queryDbModel.Status = BackgroundQueryStatus.Completed;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("52cac4f0-cbfd-4786-9530-c96b6d909515", $"The query to HttpLLMApiProviderService failed. Unhandled error.", e);
                queryDbModel.Status = BackgroundQueryStatus.Error;
                return;
            }
        }

        private async Task<IPromptContext> BuildContextAsync()
        {
            // Generate a context builder appropriate for our MainQuery
            IPromptContextBuilder contextBuilder = await contextBuilderFactory.GenerateAsync(BackgroundQuerySystemTags.main, promptContextElementBuilderFactory, storageService);
            return await contextBuilder.BuildAsync(queryDbModel.ChatId);
        }

        public async Task<bool> QueueProcessAsync()
        {
            if (queryDbModel == null)
            {
                return false;
            }

            _ = Task.Run(async () => await ProcessMainQueryAsync());

            return true;
        }

        public async Task<BackgroundQueryDbModel> GetBackgroundQueryDbModelAsync() => queryDbModel;

        /// <summary>
        /// Process the resulting completed query. If it was a 'main', it'll add a new AI message, if it was a sceneTracker, it'll attach the tracker, if it was a summary, it'll attach the summary to an existing message, etc.
        /// </summary>
        public async Task ProcessCompletedQueryAsync(BackgroundQueryDbModel selectedQuery)
        {
            if (selectedQuery == null || selectedQuery.Status != BackgroundQueryStatus.Completed)
            {
                LoggingManager.LogToFile("12498826-8f44-4f5f-ac9f-51f7de6e08fa", $"Ignoring completed background query [{selectedQuery?.BackgroundQueryId}]. Status was [{selectedQuery?.Status}].");
                return;
            }

            // Add the message
            CreateMessageQueryModel messageQueryModel = new()
            {
                ChatId = selectedQuery.ChatId,
                SourceType = MessageSourceType.AI,
                MessageContent = selectedQuery.Content,
                CreatedAtUtc = DateTime.UtcNow,
            };


            var message = await storageService.CreateMessageAsync(messageQueryModel);
            selectedQuery.LinkedMessageId = message.MessageId;
        }
    }
}
