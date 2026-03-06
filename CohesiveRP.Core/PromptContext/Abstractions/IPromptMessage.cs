using System.Text.Json.Serialization;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;

namespace CohesiveRP.Core.PromptContext.Abstractions
{
    public interface IPromptMessage
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("role")]
        OpenAIChatCompletionRole Role { get; set; }

        [JsonPropertyName("content")]
        string Content { get; set; }
    }
}
