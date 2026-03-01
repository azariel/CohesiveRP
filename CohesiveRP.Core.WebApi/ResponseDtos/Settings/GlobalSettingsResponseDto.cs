using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Settings
{
    public class GlobalSettingsResponseDto : IWebApiResponseDto
    {
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("llmProviders")]
        public string LLMProviders { get; set; }
    }
}
