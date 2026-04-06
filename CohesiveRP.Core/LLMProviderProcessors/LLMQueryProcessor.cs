using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
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
        protected BackgroundQuerySystemTags? tag;
        protected BackgroundQueryDbModel backgroundQueryDbModel;
        protected IPromptContextBuilderFactory contextBuilderFactory;
        protected IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
        protected IStorageService storageService;
        protected IHttpLLMApiProviderService httpLLMApiProviderService;
        protected ISummaryService summaryService;
        protected IPromptContextBuilder contextBuilder;
        protected IPromptContext promptContext;
        protected LLMApiResponseMessage[] messages = null;

        public LLMQueryProcessor(
            ChatCompletionPresetType completionPresetType,
            BackgroundQuerySystemTags? tag,
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

            this.promptContext = BuildContextAsync(backgroundQueryDbModel).Result;
            if (promptContext == null)
            {
                LoggingManager.LogToFile("1a5f7d13-c2ef-46ad-a2dc-726510342f4c", $"Couldn't build promptContext because not one was configured for [{completionPresetType}].");
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                return;
            }
        }

        public virtual async Task<BackgroundQueryDbModel> GetBackgroundQueryDbModelAsync() => backgroundQueryDbModel;

        public virtual async Task ProcessQueryAsync()
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

                backgroundQueryDbModel.StartFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                CancellationToken token = new CancellationTokenSource(180000).Token;
                IHttpLLMApiQueryResponseDto response = await httpLLMApiProviderService.QueryApiAsync(completionPresetType.ToString(), availableLLMApiProviders, promptContext, backgroundQueryDbModel, token);

                if (response == null)
                {
                    await Task.Delay(2000);
                    backgroundQueryDbModel.Content = "";
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// retry
                    return;
                }

                IPromptMessage[] responseMessages = response.Messages;

                backgroundQueryDbModel.Content = JsonCommonSerializer.SerializeToString(response.Messages);
                backgroundQueryDbModel.Status = BackgroundQueryStatus.ProcessingFinalInstruction;
                await storageService.UpdateBackgroundQueryAsync(backgroundQueryDbModel);
            } catch (Exception e)
            {
                await Task.Delay(2000);
                LoggingManager.LogToFile("07b4df24-1780-4a0b-a81d-97fedb852d41", $"The query to HttpLLMApiProviderService failed. Unhandled error.", e);
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                return;
            }
        }

        protected virtual async Task<IPromptContext> BuildContextAsync(BackgroundQueryDbModel backgroundQuery)
        {
            // Generate a context builder appropriate for our MainQuery
            this.contextBuilder = await contextBuilderFactory.GenerateAsync(tag ?? BackgroundQuerySystemTags.custom, promptContextElementBuilderFactory, storageService, backgroundQuery);
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

        public virtual async Task<bool> ProcessCompletedQueryAsync()
        {
            if (backgroundQueryDbModel == null || backgroundQueryDbModel.Status != BackgroundQueryStatus.ProcessingFinalInstruction)
            {
                LoggingManager.LogToFile("f26a3023-556d-43fb-b064-5f19643804a5", $"Ignoring background query [{backgroundQueryDbModel?.BackgroundQueryId}]. Status was [{backgroundQueryDbModel?.Status}].");
                return false;
            }

            if (string.IsNullOrWhiteSpace(backgroundQueryDbModel.Content))
            {
                LoggingManager.LogToFile("e4acf29a-cb5e-4de9-975a-0d508c0ad950", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content was null or empty. Task will be set to Pending status for re-generation.");
                return false;
            }

            // Deserialize the generic content into a valid Scene Analyzer
            try
            {
                messages = JsonCommonSerializer.DeserializeFromString<LLMApiResponseMessage[]>(backgroundQueryDbModel.Content);

                if (messages == null || messages.Length <= 0)
                {
                    LoggingManager.LogToFile("a86f6951-e51a-44c2-ba9d-71433b0d9407", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content no messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                    return false;
                }
            } catch (Exception e)
            {
                LoggingManager.LogToFile("be1a2097-a9d4-4242-9a9d-4e60429f59df", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                return false;
            }

            return true;
        }
    }
}
