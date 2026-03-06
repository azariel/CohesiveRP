using System.Text.Json.Serialization;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;

namespace CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Response
{
    public class OpenAIMessage
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public OpenAIChatCompletionMessage Message { get; set; }

        [JsonPropertyName("logProbs")]
        public string LogProbs { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }// TODO: map on enum
    }
}
