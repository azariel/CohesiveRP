using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects
{
    public class MessageDefinition
    {
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
