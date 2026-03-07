namespace CohesiveRP.Storage.QueryModels.Message
{
    public class CreateSummaryQueryModel
    {
        public string ChatId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string MessageIdTracker { get; set; }
    }
}
