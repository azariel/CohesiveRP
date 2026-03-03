using System.Text.Json.Serialization;
using CohesiveRP.Common.BusinessObjects;

namespace CohesiveRP.Storage.QueryModels.Message
{
    public class CreateMessageQueryModel
    {
        public string ChatId { get; set; }
        public string MessageContent { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        [JsonConverter(typeof(JsonNumberEnumConverter<MessageSourceType>))]
        public MessageSourceType SourceType { get; set; }
    }
}
