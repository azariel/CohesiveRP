using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Storage.WebApi.RequestDtos.Chat
{
    public class GetChatByIdRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }
    }
}
