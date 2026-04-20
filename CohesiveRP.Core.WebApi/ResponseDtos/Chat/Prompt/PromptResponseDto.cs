using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat.Prompt
{
    public class PromptResponseDto  : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }
    }
}
