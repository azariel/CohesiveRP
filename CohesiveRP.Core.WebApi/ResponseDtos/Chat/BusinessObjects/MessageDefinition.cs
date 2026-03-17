using System.Net;
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

        [JsonConverter(typeof(JsonNumberEnumConverter<MessageSourceType>))]

        [JsonPropertyName("sourceType")]
        public MessageSourceType SourceType { get; set; }

        [JsonPropertyName("createdAtUtc")]
        public DateTime CreatedAtUtc { get; set; }

        [JsonPropertyName("messageIndex")]
        public int MessageIndex { get; set; }

        [JsonPropertyName("summarized")]
        public bool Summarized { get; set; }

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("characterName")]
        public string CharacterName { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }

        [JsonPropertyName("personaName")]
        public string PersonaName { get; set; }
    }
}
