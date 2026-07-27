using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.SamplingSettings;

namespace CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request
{
    public class OpenAIChatCompletionRequestDto : OpenAISamplingSettings
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public OpenAIChatCompletionMessage[] Messages { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }
    }
}
