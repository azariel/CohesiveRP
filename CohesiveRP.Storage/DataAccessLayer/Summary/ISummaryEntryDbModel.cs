namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public interface ISummaryEntryDbModel
    {
        string Content { get; set; }
        DateTime CreatedAtUtc { get; set; }
        string MessageIdTracker { get; set; }
    }
}
