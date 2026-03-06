using System.Text.Json.Serialization;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;

namespace CohesiveRP.Core.Services.LLMApiProvider
{
    /// <summary>
    /// Basic generic model implementing IPromptMessage for deserialization purpose.
    /// </summary>
    public class LLMApiResponseMessage : IPromptMessage
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("role")]
        public OpenAIChatCompletionRole Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
