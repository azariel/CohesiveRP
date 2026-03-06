using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.PromptContext.Abstractions;

namespace CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Response
{
    public class OpenAIChatCompletionResponseDto : IHttpLLMApiQueryResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; } = "chat.compeltion";

        [JsonPropertyName("created")]
        public long CreatedAtUtcInSeconds { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("choices")]
        public OpenAIMessage[] Messages { get; set; }

        [JsonPropertyName("usage")]
        public OpenAIUsage Usage { get; set; }

        [JsonPropertyName("service_tier")]
        public string ServiceTier { get; set; }

        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        IPromptMessage[] IHttpLLMApiQueryResponseDto.Messages => Messages?.Select(s => s.Message).ToArray();
    }
}
