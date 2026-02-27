namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public interface IMessageDbModel
    {
        string MessageId { get; set; }
        string Content { get; set; }
    }
}
