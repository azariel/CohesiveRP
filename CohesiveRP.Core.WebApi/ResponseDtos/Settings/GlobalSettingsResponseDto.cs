using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.BusinessObjects.LLMProviders;
using CohesiveRP.Core.ChatCompletionPresets;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Settings
{
    public class GlobalSettingsResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("llmProviders")]
        public List<LLMProviderConfig> LLMProviders { get; set; } = new();

        [JsonPropertyName("chatCompletionPresetsMap")]
        public ChatCompletionPresetsMap ChatCompletionPresetsMap { get; set; } = new();
    }
}
