using System.Net;
using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects
{
    public class ChatDefinition
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("name")]
        public string ChatName { get; set; }

        [JsonPropertyName("avatarFilePath")]
        public string AvatarFilePath { get; set; }

        [JsonPropertyName("lastActivityAtUtc")]
        public DateTime LastActivityAtUtc { get; set; }

        [JsonPropertyName("characterIds")]
        public List<string> CharacterIds { get; set; }

        [JsonPropertyName("lorebookIds")]
        public List<string> LorebookIds { get; set; }
    }
}
