using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.TimeoutStrategies
{
    public class TimeoutStrategy
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("type")]
        public LLMProviderTimeoutStrategyType Type { get; set; }

        [JsonPropertyName("retries")]
        public int Retries { get; set; } = 3;
    }
}
