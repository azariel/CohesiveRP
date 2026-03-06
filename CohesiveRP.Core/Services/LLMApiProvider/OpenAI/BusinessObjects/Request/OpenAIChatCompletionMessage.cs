using System.Text.Json.Serialization;
using CohesiveRP.Core.PromptContext.Abstractions;

namespace CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request
{
    public class OpenAIChatCompletionMessage : IPromptMessage
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("role")]
        public OpenAIChatCompletionRole Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
