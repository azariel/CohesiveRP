using System.Text.Json.Serialization;
using CohesiveRP.Common.BusinessObjects;

namespace CohesiveRP.Storage.QueryModels.Message
{
    public class CreateMessageQueryModel
    {
        public string ChatId { get; set; }
        public string MessageContent { get; set; }
        public string MessageThink { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        [JsonConverter(typeof(JsonNumberEnumConverter<MessageSourceType>))]
        public MessageSourceType SourceType { get; set; }
        public bool Summarized { get; set; }
        public string CharacterId { get; set; }
        public string[] AvatarsFilePath { get; set; }
        public DateTime? InRoleplayDateTime { get; set; }
        public DateTime? StartGenerationDateTimeUtc { get; set; }
        public DateTime? StartFocusedGenerationDateTimeUtc { get; set; }
        public DateTime? EndFocusedGenerationDateTimeUtc { get; set; }
    }
}
