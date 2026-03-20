using System.Text.Json.Serialization;
using CohesiveRP.Common.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public class MessageDbModel : IMessageDbModel
    {
        public string MessageId { get; set; }

        public string Content { get; set; }


        [JsonConverter(typeof(JsonNumberEnumConverter<MessageSourceType>))]
        public MessageSourceType SourceType { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public bool Summarized { get; set; }

        public string CharacterId { get; set; }

        public string AvatarId { get; set; }
    }
}
