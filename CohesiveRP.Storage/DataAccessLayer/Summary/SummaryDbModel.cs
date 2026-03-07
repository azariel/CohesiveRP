using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Storage.DataAccessLayer.Summary
{
    public class SummaryDbModel : ISummaryDbModel
    {
        /// <summary>
        /// The most recent message Id processed by this summary model
        /// </summary>
        public string MessageIdTracker { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
