using System.Text.Json.Serialization;

namespace CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request
{
    public class OpenAIChatCompletionRequestDto
    {
        [JsonPropertyName("model")]
        public string Model {get; set; }

        [JsonPropertyName("messages")]
        public OpenAIChatCompletionMessage[] Messages {get; set; }
    }
}
