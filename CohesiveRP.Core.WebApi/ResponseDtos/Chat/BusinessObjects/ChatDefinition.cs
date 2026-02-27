using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects
{
    public class ChatDefinition
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        // TODO: Name, desc, avatar
    }
}
