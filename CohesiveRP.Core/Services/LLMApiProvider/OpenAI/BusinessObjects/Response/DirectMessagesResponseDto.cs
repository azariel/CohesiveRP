using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;

namespace CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Response
{
    public class DirectMessagesResponseDto : IHttpLLMApiQueryResponseDto
    {
        [JsonPropertyName("choices")]
        public OpenAIChatCompletionMessage[] Messages { get; set; }

        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        IPromptMessage[] IHttpLLMApiQueryResponseDto.Messages => Messages?.Select(s => s).ToArray();
    }
}
