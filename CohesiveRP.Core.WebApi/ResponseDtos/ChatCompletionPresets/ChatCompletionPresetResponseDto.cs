using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets
{
    public class ChatCompletionPresetResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("chatCompletionPreset")]
        public ChatCompletionPreset ChatCompletionPreset { get; set; }
    }
}
