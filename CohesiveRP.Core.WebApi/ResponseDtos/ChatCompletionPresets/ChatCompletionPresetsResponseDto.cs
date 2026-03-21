using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets
{
    public class ChatCompletionPresetsResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("chatCompletionPresets")]
        public ChatCompletionAllPresets[] ChatCompletionPresetsMap { get; set; } = [];
    }
}
