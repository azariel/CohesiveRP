using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat
{
    public class ChatCreationResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("name")]
        public string ChatName { get; set; }
    }
}
