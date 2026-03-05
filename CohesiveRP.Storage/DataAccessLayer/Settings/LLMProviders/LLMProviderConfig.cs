using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.TimeoutStrategies;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders
{
    public class LLMProviderConfig
    {
        [JsonPropertyName("providerConfigId")]
        public string ProviderConfigId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "Unknown";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("type")]
        public LLMProviderType Type { get; set; }

        [JsonPropertyName("concurrencyLimit")]
        public int ConcurrencyLimit { get; set; } = 1;

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; } = ["default"];

        [JsonPropertyName("timeoutStrategy")]
        public TimeoutStrategy TimeoutStrategy { get; set; } = new();
    }
}
