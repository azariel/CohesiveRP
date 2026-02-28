using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.QueryModels.Message
{
    public class CreateMessageQueryModel
    {
        public string ChatId { get; set; }
        public string MessageContent { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}
