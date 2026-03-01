using System.Text.Json.Serialization;

namespace CohesiveRP.Core.BusinessObjects.LLMProviders.TimeoutStrategies
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
