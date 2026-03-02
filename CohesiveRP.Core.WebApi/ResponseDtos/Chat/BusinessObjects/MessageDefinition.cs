using System.Text.Json.Serialization;
using CohesiveRP.Common.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects
{
    public class MessageDefinition
    {
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("sourceType")]
        public MessageSourceType SourceType { get; set; }

        [JsonPropertyName("createdAtUtc")]
        public DateTime CreatedAtUtc { get; set; }
    }
}
