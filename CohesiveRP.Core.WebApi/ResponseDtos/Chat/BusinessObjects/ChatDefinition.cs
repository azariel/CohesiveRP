using System.Net;
using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects
{
    public class ChatDefinition
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("avatarCharacterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("name")]
        public string ChatName { get; set; }
    }
}
