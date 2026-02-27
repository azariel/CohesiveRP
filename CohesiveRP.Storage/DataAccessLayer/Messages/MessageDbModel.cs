namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public class MessageDbModel : IMessageDbModel
    {
        public string MessageId { get;set; }
        public string Content { get;set; }
    }
}
