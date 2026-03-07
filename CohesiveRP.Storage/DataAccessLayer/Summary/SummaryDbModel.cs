using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Storage.DataAccessLayer.Summary
{
    public class SummaryDbModel : ISummaryDbModel
    {
        public string Content { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
