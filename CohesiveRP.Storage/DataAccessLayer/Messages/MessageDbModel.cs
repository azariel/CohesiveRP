using System.Text.Json.Serialization;
using CohesiveRP.Common.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public class MessageDbModel : IMessageDbModel
    {
        public string MessageId { get; set; }

        public string Content { get; set; }
        public string ThinkingContent { get; set; }

        [JsonConverter(typeof(JsonNumberEnumConverter<MessageSourceType>))]
        public MessageSourceType SourceType { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public bool Summarized { get; set; }

        public string CharacterId { get; set; }

        public string[] AvatarsFilePath { get; set; }

        public DateTime? InRoleplayDateTime { get; set; }

        public DateTime? StartGenerationDateTimeUtc { get; set; }
        public DateTime? StartFocusedGenerationDateTimeUtc { get; set; }
        public DateTime? EndFocusedGenerationDateTimeUtc { get; set; }
    }
}
