using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.HttpClient;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services.ErrorHandlers;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Response;
using CohesiveRP.Core.Services.LLMApiProvider.Utils;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;

namespace CohesiveRP.Core.Services
{
    public class HttpLLMApiProviderService : IHttpLLMApiProviderService
    {
        private IStorageService storageService;
        private ILLMApiQueryPayloadBuilderFactory llmApiQueryPayloadBuilderFactory;
        private List<LLMApiProviderErrorState> llmApiProviderErrorStates = new();

        public HttpLLMApiProviderService(IStorageService storageService, ILLMApiQueryPayloadBuilderFactory llmApiQueryPayloadBuilderFactory)
        {
            this.storageService = storageService;
            this.llmApiQueryPayloadBuilderFactory = llmApiQueryPayloadBuilderFactory;
        }

        private void DecrementLLMApiProviderErrorState(LLMProviderConfig providerConfig)
        {
            var errorState = llmApiProviderErrorStates.FirstOrDefault(f => f.ProviderConfigId == providerConfig.ProviderConfigId);
            if (errorState != null && errorState.ErrorsBalance > 0)
            {
                errorState.ErrorsBalance--;
                errorState.TimeoutUntilDateTimeUtc = null;
            }
        }

        private void IncrementLLMApiProviderErrorState(LLMProviderConfig providerConfig)
        {
            var errorState = llmApiProviderErrorStates.FirstOrDefault(f => f.ProviderConfigId == providerConfig.ProviderConfigId);
            if (errorState == null)
            {
                llmApiProviderErrorStates.Add(new LLMApiProviderErrorState()
                {
                    ProviderConfigId = providerConfig.ProviderConfigId,
                    ErrorsBalance = 1
                });
            } else
            {
                errorState.ErrorsBalance++;

                if (providerConfig.ErrorsBehavior != null && providerConfig.ErrorsBehavior.NbErrorsBeforeTimeout <= errorState.ErrorsBalance)
                {
                    errorState.TimeoutUntilDateTimeUtc = DateTime.UtcNow.AddSeconds(providerConfig.ErrorsBehavior.TimeoutInSeconds);
                }
            }
        }

        private LLMProviderConfig SelectLLMApiProviderConfigFromContext(LLMApiQueryDbModel[] ongoingQueriesRunningAgainstLLMApis, LLMProviderConfig[] allConfigs, LLMProviderConfig[] availableConfigs)
        {
            LLMProviderConfig selectedLLMApiQueryDbModel = null;
            foreach (LLMProviderConfig config in availableConfigs)
            {
                var selectedLLmApiProviderErrorState = llmApiProviderErrorStates.FirstOrDefault(f => f.ProviderConfigId == config.ProviderConfigId);
                var maxmimalAmountOfErrorsAllowedInMainPath = 100;
                
                if(config.FallbackStrategies != null && config.FallbackStrategies.Count > 0)
                    maxmimalAmountOfErrorsAllowedInMainPath = config.FallbackStrategies.Min(o => o.ErrorsTreshold);

                if (selectedLLmApiProviderErrorState?.TimeoutUntilDateTimeUtc != null && selectedLLmApiProviderErrorState.TimeoutUntilDateTimeUtc <= DateTime.UtcNow)
                {
                    selectedLLmApiProviderErrorState.TimeoutUntilDateTimeUtc = null;
                    selectedLLmApiProviderErrorState.ErrorsBalance--;
                }

                // Is the maximal amount of simulteanous queries running on this Api reached
                if (ongoingQueriesRunningAgainstLLMApis.Count(w => w.LLMProviderConfigId == config.ProviderConfigId) < config.ConcurrencyLimit &&
                    (selectedLLmApiProviderErrorState == null || (selectedLLmApiProviderErrorState.ErrorsBalance < maxmimalAmountOfErrorsAllowedInMainPath && (selectedLLmApiProviderErrorState.TimeoutUntilDateTimeUtc == null || selectedLLmApiProviderErrorState.TimeoutUntilDateTimeUtc <= DateTime.UtcNow))))
                {
                    // This provider is good to go, select it
                    selectedLLMApiQueryDbModel = config;
                    break;
                }

                // Is the provider temporarily unavailable due to a timeout strategy?
                if (config.FallbackStrategies != null && config.FallbackStrategies.Count > 0)
                {
                    foreach (var fallbackStrategy in config.FallbackStrategies.OrderBy(o => o.ErrorsTreshold))
                    {
                        if (selectedLLmApiProviderErrorState.ErrorsBalance >= fallbackStrategy.ErrorsTreshold)
                        {
                            // The treshold to use this fallback was reached
                            // Although, if the concurrency limit of this provider is already reached, we should still skip it
                            var selectedConfig = allConfigs.FirstOrDefault(w => w.ProviderConfigId == fallbackStrategy.ProviderConfigId);
                            if (selectedConfig == null || ongoingQueriesRunningAgainstLLMApis.Count(w => w.LLMProviderConfigId == fallbackStrategy.ProviderConfigId) >= selectedConfig.ConcurrencyLimit)
                            {
                                // skip this fallback provider because it's not available (already processing max amount of concurrent queries), but we should keep it in consideration for the next queries
                                continue;
                            }

                            // If that provider had many errors, skip it as well
                            LLMApiProviderErrorState selectedLLmApiProviderErrorStateFallback = llmApiProviderErrorStates.FirstOrDefault(f => f.ProviderConfigId == fallbackStrategy.ProviderConfigId);
                            if (selectedLLmApiProviderErrorStateFallback != null)
                            {
                                if (selectedLLmApiProviderErrorStateFallback.TimeoutUntilDateTimeUtc > DateTime.UtcNow)
                                {
                                    selectedLLmApiProviderErrorStateFallback.TimeoutUntilDateTimeUtc = null;
                                    selectedLLmApiProviderErrorStateFallback.ErrorsBalance--;
                                }

                                if (selectedLLmApiProviderErrorStateFallback.ErrorsBalance >= fallbackStrategy.ErrorsTresholdBelowXToAllowFallback ||
                                    (selectedLLmApiProviderErrorStateFallback.TimeoutUntilDateTimeUtc != null && selectedLLmApiProviderErrorStateFallback.TimeoutUntilDateTimeUtc > DateTime.UtcNow))
                                {
                                    continue;
                                }
                            }

                            // otherwise, that fallback is valid
                            selectedLLMApiQueryDbModel = selectedConfig;
                        }
                    }
                }
            }

            // absolute fallback
            //if(selectedLLMApiQueryDbModel == null)
            //{
            //    return availableConfigs.FirstOrDefault();
            //}

            return selectedLLMApiQueryDbModel;
        }

        public async Task<IHttpLLMApiQueryResponseDto> QueryApiAsync(string tag, LLMProviderConfig[] globalLLMApiProviders, LLMProviderConfig[] availableLLMApiProviders, IPromptContext promptContext, BackgroundQueryDbModel backgroundQueryDbModel, CancellationToken token)
        {
            if (availableLLMApiProviders == null || availableLLMApiProviders.Length <= 0)
            {
                LoggingManager.LogToFile("59f8b42e-60fc-4553-a7ec-fa640bc898a1", $"Couldn't query a LLM Api because no one was configured for tag [{tag}].");
            }

            // filter a list with the available api with less than their concurrencyLimit of queries ongoing, then take the one with top priority and queue our new query on it
            // Get the state of the ongoing queries to LLM Apis
            LLMApiQueryDbModel[] ongoingQueriesRunningAgainstLLMApis = await storageService.GetQueriesOnLLMApisAsync(tag);
            ongoingQueriesRunningAgainstLLMApis = ongoingQueriesRunningAgainstLLMApis?.Where(w => w.Status == Storage.DataAccessLayer.LLMApiQueries.BusinessObjects.LLMApiQueryStatus.Running).ToArray();

            List<LLMProviderConfig> availableConfigs = [.. availableLLMApiProviders];
            availableConfigs = availableConfigs.OrderBy(o => o.Priority).ToList();

            // Check the running queries to know our concurrency situation
            LLMApiQueryDbModel[] llmQueries = await storageService.GetQueriesOnLLMApisAsync(tag);
            llmQueries = llmQueries.Where(w => w.Status == Storage.DataAccessLayer.LLMApiQueries.BusinessObjects.LLMApiQueryStatus.Running).ToArray();

            // Parse them one by one (ordered by priority) to know if our current Task can use that Api
            var selectedLLMApiQueryDbModel = SelectLLMApiProviderConfigFromContext(ongoingQueriesRunningAgainstLLMApis, globalLLMApiProviders, availableLLMApiProviders);
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

            try
            {
                // Now once we have a state in Db for our query, we can poke the actual inference server api
                var LLMApiResult = await PostLLMApiAsync(selectedLLMApiQueryDbModel, promptContext, backgroundQueryDbModel, token);

                if (LLMApiResult == null)
                {
                    IncrementLLMApiProviderErrorState(selectedLLMApiQueryDbModel);
                } else
                {
                    DecrementLLMApiProviderErrorState(selectedLLMApiQueryDbModel);
                }

                return LLMApiResult;

                // TODO: Manage the state of the ongoing query to the inference server api
                // TODO: if streaming, then use a delegate to update the content as we receive it

            } catch (Exception ex)
            {
                LoggingManager.LogToFile("f1e27326-2284-488e-bbe3-66a8f1ba47c7", $"An LLM Api Query failed on tag [{tag}]. Unhandled error.", ex);
                IncrementLLMApiProviderErrorState(selectedLLMApiQueryDbModel);
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
        }

        private static string? TryExtractContentDelta(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);

                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("delta")
                    .TryGetProperty("content", out JsonElement content)
                        ? content.GetString()
                        : null;
            } catch (Exception)
            {
                return null; // malformed frame — skip silently
            }
        }

        private async Task<IHttpLLMApiQueryResponseDto> PostLLMApiAsync(LLMProviderConfig selectedLLMApiQueryDbModel, IPromptContext promptContext, BackgroundQueryDbModel backgroundQueryDbModel, CancellationToken token)
        {
            if (selectedLLMApiQueryDbModel == null || string.IsNullOrWhiteSpace(selectedLLMApiQueryDbModel.Model) || string.IsNullOrWhiteSpace(selectedLLMApiQueryDbModel.ApiUrl))
            {
                LoggingManager.LogToFile("26968ad4-8437-4819-8cbb-35e6da04020f", $"The configured LLMProviderConfig [{selectedLLMApiQueryDbModel.ProviderConfigId}] is incorrectly configured.");
                return null;
            }

            using HttpRestClient httpClient = new HttpRestClient();

            ILLMApiQueryPayloadBuilder llmApiQueryPayloadBuilder = llmApiQueryPayloadBuilderFactory.Create(selectedLLMApiQueryDbModel.Type);
            string payload = llmApiQueryPayloadBuilder.BuildPayload(promptContext, selectedLLMApiQueryDbModel);

            try
            {
                if (selectedLLMApiQueryDbModel.Stream)
                {
                    StringBuilder str = new();
                    await foreach (string chunk in httpClient.PostStreamAsync(selectedLLMApiQueryDbModel.ApiUrl, payload, token))
                    {
                        var content = ParseNextLLMStreamedContent(chunk);
                        str.Append(content);

                        if (backgroundQueryDbModel != null)
                        {
                            backgroundQueryDbModel.Content += content;
                            await storageService.UpdateBackgroundQueryAsync(backgroundQueryDbModel);
                        }
                    }

                    IHttpLLMApiQueryResponseDto httpLLMApiQueryResponseDto = new DirectMessagesResponseDto()
                    {
                        HttpResultCode = System.Net.HttpStatusCode.OK,
                        Messages = [
                           new OpenAIChatCompletionMessage
                           {
                               Role = OpenAIChatCompletionRole.assistant,
                               Content = str.ToString(),
                           }
                        ],
                    };
                    return httpLLMApiQueryResponseDto;
                } else
                {
                    string rawResponse = await httpClient.PostAsync(selectedLLMApiQueryDbModel.ApiUrl, payload, token);
                    IHttpLLMApiQueryResponseDto httpLLMApiQueryResponseDto = LLMApiQueryResponseDtoConverter.Convert(selectedLLMApiQueryDbModel.Type, rawResponse);
                    return httpLLMApiQueryResponseDto;
                }

            } catch (Exception e)
            {
                if (e.Message.Contains("input should be a valid string", StringComparison.InvariantCultureIgnoreCase))
                {
                    LoggingManager.LogToFile("611e883a-f3c6-4e9a-9f35-458690125ede", $"The configured LLMProviderConfig [{selectedLLMApiQueryDbModel.ProviderConfigId}] is incorrectly configured. The OpenAI compliant inference server has refused to serve the request due to an incorrect configuration.");
                }

                if (e.Message.Contains("no connection could be made because the target machine actively refused it", StringComparison.InvariantCultureIgnoreCase))
                {
                    LoggingManager.LogToFile("4b4683cf-e3a1-4494-8ecd-1e288bb61e81", $"Can't execute Http request. The requested Api is down. ApiUrl: [{selectedLLMApiQueryDbModel.ApiUrl}].");
                }

                LoggingManager.LogToFile("dd346c77-ea60-463b-8dda-ed7c95d62757", $"LLM query failed. Exception:[{e.Message}].");

                return null;
            }
        }

        private string ParseNextLLMStreamedContent(string rawChunk)
        {
            if (string.IsNullOrWhiteSpace(rawChunk))
                return "";

            if (!rawChunk.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return "";

            string payload = rawChunk["data:".Length..].Trim();

            string chunk = TryExtractContentDelta(payload);

            if (chunk is not null)
                return chunk;

            return "";
        }
    }
}
