using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.PromptContext.Abstractions;

namespace CohesiveRP.Core.HttpLLMApiProvider
{
    public interface IHttpLLMApiQueryResponseDto : IWebApiResponseDto
    {
        [JsonPropertyName("messages")]
        public IPromptMessage[] Messages { get; }
    }
}
