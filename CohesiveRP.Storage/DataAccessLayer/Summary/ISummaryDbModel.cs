namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public interface ISummaryDbModel
    {
        string Content { get; set; }
        DateTime CreatedAtUtc { get; set; }
    }
}
