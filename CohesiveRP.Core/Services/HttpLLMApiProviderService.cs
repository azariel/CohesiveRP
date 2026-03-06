using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.HttpClient;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;

namespace CohesiveRP.Core.Services
{
    public class HttpLLMApiProviderService : IHttpLLMApiProviderService
    {
        private IStorageService storageService;
        private ILLMApiQueryPayloadBuilderFactory llmApiQueryPayloadBuilderFactory;

        public HttpLLMApiProviderService(IStorageService storageService, ILLMApiQueryPayloadBuilderFactory llmApiQueryPayloadBuilderFactory)
        {
            this.storageService = storageService;
            this.llmApiQueryPayloadBuilderFactory = llmApiQueryPayloadBuilderFactory;
        }

        public async Task<HttpLLMApiProviderQueryResponseDto> QueryApiAsync(string tag, LLMProviderConfig[] availableLLMApiProviders, IPromptContext promptContext)
        {
            if (availableLLMApiProviders == null || availableLLMApiProviders.Length <= 0)
            {
                LoggingManager.LogToFile("59f8b42e-60fc-4553-a7ec-fa640bc898a1", $"Couldn't query a LLM Api because no one was configured for tag [{tag}].");
            }

            // filter a list with the available api with less than their concurrencyLimit of queries ongoing, then take the one with top priority and queue our new query on it
            // Get the state of the ongoing queries to LLM Apis
            LLMApiQueryDbModel[] ongoingQueriesRunningAgainstLLMApis = await storageService.GetQueriesOnLLMApisAsync(tag);

            List<LLMProviderConfig> availableConfigs = [.. availableLLMApiProviders];
            availableConfigs = availableConfigs.OrderBy(o => o.Priority).ToList();

            // Parse them one by one (ordered by priority) to know if our current Task can use that Api
            LLMProviderConfig selectedLLMApiQueryDbModel = null;
            foreach (LLMProviderConfig config in availableConfigs)
            {
                if (config.ConcurrencyLimit < ongoingQueriesRunningAgainstLLMApis.Count(w => w.LLMProviderConfigId == config.ProviderConfigId))
                {
                    continue;
                }

                // This Api is available to handle our query, let's reserve it
                selectedLLMApiQueryDbModel = config;
            }

            if (selectedLLMApiQueryDbModel == null)
            {
                LoggingManager.LogToFile("a9cdc236-eee5-45bb-95ff-08a0dd9b0716", $"There was no LLM Api available to process our query for tag [{tag}].");

                // TODO: What do we do? We should have an enum with fallback behavior. #1) Queue to top priotiy #2) Queue to lowest used #3) throw?
                return null;
            }

            // Add a new state in Db to represent the fact that we have a new Http query running on this specific inference server Api
            LLMApiQueryDbModel newQuery = new()
            {
                LLMApiQueryId = Guid.NewGuid().ToString(),
                LLMProviderConfigId = selectedLLMApiQueryDbModel.ProviderConfigId,
                Status = Storage.DataAccessLayer.LLMApiQueries.BusinessObjects.LLMApiQueryStatus.Running,
                Tag = tag,
            };

            LLMApiQueryDbModel addResponse = await storageService.AddNewQueryAsync(newQuery);

            if (addResponse == null)
            {
                LoggingManager.LogToFile("5d617262-d527-4cef-88cd-4fa7da8891ad", $"Couldn't add a new LLM Api Query to Storage for tag [{tag}]. Unhandled error.");
                return null;
            }

            HttpLLMApiProviderQueryResponseDto response = null;
            try
            {
                // Now once we have a state in Db for our query, we can poke the actual inference server api
                var LLMApiResult = await PostLLMApiAsync(selectedLLMApiQueryDbModel, promptContext);

                // TODO: Manage the state of the ongoing query to the inference server api
                // TODO: if streaming, then use a delegate to update the content as we receive it

            } catch (Exception ex)
            {
                LoggingManager.LogToFile("f1e27326-2284-488e-bbe3-66a8f1ba47c7", $"An LLM Api Query failed on tag [{tag}]. Unhandled error.", ex);
                return null;
            } finally
            {
                // TODO: wrap this in a for i=10
                bool deleteResponse = await storageService.DeleteQueryByIdAsync(newQuery.LLMApiQueryId);

                if (addResponse == null)
                {
                    LoggingManager.LogToFile("5d617262-d527-4cef-88cd-4fa7da8891ad", $"Couldn't add a new LLM Api Query to Storage for tag [{tag}]. Unhandled error.");
                }
            }

            return response;
        }

        private async Task<bool> PostLLMApiAsync(LLMProviderConfig selectedLLMApiQueryDbModel, IPromptContext promptContext)
        {
            if (selectedLLMApiQueryDbModel == null || string.IsNullOrWhiteSpace(selectedLLMApiQueryDbModel.Model) || string.IsNullOrWhiteSpace(selectedLLMApiQueryDbModel.ApiUrl))
            {
                LoggingManager.LogToFile("26968ad4-8437-4819-8cbb-35e6da04020f", $"The configured LLMProviderConfig [{selectedLLMApiQueryDbModel.ProviderConfigId}] is incorrectly configured.");
                return false;
            }

            using HttpRestClient httpClient = new HttpRestClient();

            ILLMApiQueryPayloadBuilder llmApiQueryPayloadBuilder = llmApiQueryPayloadBuilderFactory.Create(selectedLLMApiQueryDbModel.Type);
            string payload = llmApiQueryPayloadBuilder.BuildPayload(promptContext, selectedLLMApiQueryDbModel.Model);

            string result = null;

            try
            {
                result = await httpClient.PostAsync(selectedLLMApiQueryDbModel.ApiUrl, payload);
                var
            } catch (Exception e)
            {
                if (e.Message.ToLowerInvariant().Contains("input should be a valid string"))
                {
                    LoggingManager.LogToFile("611e883a-f3c6-4e9a-9f35-458690125ede", $"The configured LLMProviderConfig [{selectedLLMApiQueryDbModel.ProviderConfigId}] is incorrectly configured. The OpenAI compliant inference server has refused to serve the request due to an incorrect configuration.");
                }
                
                return false;
            }

            return true;
        }
    }
}
