using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.LLMApiQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.FallbackStrategies;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.TimeoutStrategies;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders
{
    public class LLMProviderConfig
    {
        [JsonPropertyName("providerConfigId")]
        public string ProviderConfigId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "Unknown";

        [JsonPropertyName("apiUrl")]
        public string ApiUrl { get; set; } = "URL";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("type")]
        public LLMProviderType Type { get; set; }

        [JsonPropertyName("concurrencyLimit")]
        public int ConcurrencyLimit { get; set; } = 1;

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("priority")]
        public LLMProviderPriority Priority { get; set; } = LLMProviderPriority.Standard; // The background modules will use the higher priority LLM config when they aren't at their concurrencyLimit. They will fallback to lower priority ones otherwise

        [JsonPropertyName("tags")]
        public List<ChatCompletionPresetType> Tags { get; set; } = [ChatCompletionPresetType.Main];

        [JsonPropertyName("timeoutStrategy")]
        public TimeoutStrategy TimeoutStrategy { get; set; } = new();

        [JsonPropertyName("fallbackStrategies")]
        public List<FallbackStrategy> FallbackStrategies { get; set; } = new();

        [JsonPropertyName("errorsBehavior")]
        public ErrorBehavior ErrorsBehavior { get; set; } = new();
    }
}
