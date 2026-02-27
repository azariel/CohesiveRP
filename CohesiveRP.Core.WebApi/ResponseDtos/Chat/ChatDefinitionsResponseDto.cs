using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat
{
    public class ChatDefinitionsResponseDto : IWebApiResponseDto
    {
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("chats")]
        public List<ChatDefinition> Chats { get; set; } = new();
    }
}
