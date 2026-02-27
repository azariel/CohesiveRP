using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat
{
    public class MessagesResponseDto : IWebApiResponseDto
    {
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("messages")]
        public MessageDefinition[] Messages { get; set; }
    }
}
