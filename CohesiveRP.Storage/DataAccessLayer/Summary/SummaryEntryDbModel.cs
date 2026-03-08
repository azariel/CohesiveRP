using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Storage.DataAccessLayer.Summary
{
    public class SummaryEntryDbModel : ISummaryEntryDbModel
    {
        public string SummaryEntryId { get; set; }

        /// <summary>
        /// The most recent message Id processed by this summary model
        /// </summary>
        public string MessageIdTracker { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
