using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Response;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderManager
{
    public abstract class LLMQueryProcessor : ILLMQueryProcessor
    {
        protected ChatCompletionPresetType completionPresetType;
        protected BackgroundQuerySystemTags tag;
        protected BackgroundQueryDbModel backgroundQueryDbModel;
        protected IPromptContextBuilderFactory contextBuilderFactory;
        protected IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
        protected IStorageService storageService;
        protected IHttpLLMApiProviderService httpLLMApiProviderService;
        protected ISummaryService summaryService;
        protected IPromptContextBuilder contextBuilder;
        protected IPromptContext promptContext;

        public LLMQueryProcessor(
            ChatCompletionPresetType completionPresetType,
            BackgroundQuerySystemTags tag,
            BackgroundQueryDbModel backgroundQueryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService,
            ISummaryService summaryService)
        {
            this.completionPresetType = completionPresetType;
            this.tag = tag;
            this.backgroundQueryDbModel = backgroundQueryDbModel;
            this.contextBuilderFactory = contextBuilderFactory;
            this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
            this.storageService = storageService;
            this.httpLLMApiProviderService = httpLLMApiProviderService;
            this.summaryService = summaryService;

            this.promptContext = BuildContextAsync(backgroundQueryDbModel.LinkedId).Result;
            if (promptContext == null)
            {
                LoggingManager.LogToFile("1a5f7d13-c2ef-46ad-a2dc-726510342f4c", $"Couldn't build promptContext because not one was configured for [{completionPresetType}].");
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                return;
            }
        }

        public virtual async Task<BackgroundQueryDbModel> GetBackgroundQueryDbModelAsync() => backgroundQueryDbModel;

        protected virtual async Task ProcessQueryAsync()
        {
            if (backgroundQueryDbModel.Status != BackgroundQueryStatus.InProgress || promptContext == null)
            {
                return;
            }

            try
            {
                GlobalSettingsDbModel globalSettings = await storageService.GetGlobalSettingsAsync();
                LLMProviderConfig[] availableLLMApiProviders = globalSettings.LLMProviders.Where(w => w.Tags.Contains(completionPresetType)).ToArray();

                if (availableLLMApiProviders == null || availableLLMApiProviders.Length <= 0)
                {
                    LoggingManager.LogToFile("834a0e28-4ec7-4e04-a78b-d8bd6113d6bb", $"Couldn't query a LLM Api because not one was configured for [{completionPresetType}].");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return;
                }

                IHttpLLMApiQueryResponseDto response = await httpLLMApiProviderService.QueryApiAsync(completionPresetType.ToString(), availableLLMApiProviders, promptContext);
                //IHttpLLMApiQueryResponseDto response = new OpenAIChatCompletionResponseDto()
                //{
                //    HttpResultCode = System.Net.HttpStatusCode.OK,
                //    Messages = new List<OpenAIMessage>()
                //    {
                //        new OpenAIMessage()
                //        {
                //            Message = new Services.LLMApiProvider.OpenAI.BusinessObjects.Request.OpenAIChatCompletionMessage
                //            {
                //                Role = Services.LLMApiProvider.OpenAI.BusinessObjects.Request.OpenAIChatCompletionRole.assistant,
                //                Content = "e144df85-8280-4b4a-b3f1-d6b41ce930e0",
                //            },
                //            FinishReason = "",
                //            Index = 0,
                //        }
                //    }.ToArray()
                //};
                //await Task.Delay(180000);

                if (response == null)
                {
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    return;
                }

                IPromptMessage[] responseMessages = response.Messages;

                backgroundQueryDbModel.Content = JsonCommonSerializer.SerializeToString(response.Messages);
                backgroundQueryDbModel.Status = BackgroundQueryStatus.ProcessingFinalInstruction;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("07b4df24-1780-4a0b-a81d-97fedb852d41", $"The query to HttpLLMApiProviderService failed. Unhandled error.", e);
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                return;
            }
        }

        protected virtual async Task<IPromptContext> BuildContextAsync(string backgroundQueryLinkedId)
        {
            // Generate a context builder appropriate for our MainQuery
            this.contextBuilder = await contextBuilderFactory.GenerateAsync(tag, promptContextElementBuilderFactory, storageService, backgroundQueryLinkedId);
            return await contextBuilder.BuildAsync(backgroundQueryDbModel.ChatId);
        }

        public async Task<bool> QueueProcessAsync()
        {
            if (backgroundQueryDbModel == null)
            {
                return false;
            }

            _ = Task.Run(async () => await ProcessQueryAsync());

            return true;
        }

        public abstract Task ProcessCompletedQueryAsync();
    }
}
