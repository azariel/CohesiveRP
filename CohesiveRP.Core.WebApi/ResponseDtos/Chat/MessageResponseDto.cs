using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat
{
    public class MessageResponseDto : IWebApiResponseDto
    {
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("messageObj")]
        public MessageDefinition Message { get; set; }
    }
}
