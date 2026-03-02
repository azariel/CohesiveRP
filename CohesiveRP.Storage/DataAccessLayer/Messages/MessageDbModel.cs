using CohesiveRP.Common.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public class MessageDbModel : IMessageDbModel
    {
        public string MessageId { get; set; }
        public string Content { get; set; }
        public MessageSourceType SourceType { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
