using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Summary;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    /// <summary>
    /// Represents the structure of summaries in db.
    /// </summary>
    [Table("Summaries")]
    public class SummaryDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string SummaryId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }

        /// <summary>
        /// On each turn, the AI will (if configured) scan all the summaries to keep only the information relevant to the CURRENT story context.
        /// it'll ignore irrelevant information, thus reducing the summaries footprint and reducing prompt context bloat so that the LLM can 'focus' on more relevant information.
        /// It is critical that the prompt generation this field is exhaustive so that the LLM doesn't miss critical information. Note that we can't compute this field as soon as a new summary is added since it *depend* on the story CONTEXT, so we need to regenerate it EVERY SINGLE TURN for it to be accurate. To avoid desync, we need to flush this field when a new MAIN is generated so that it doesn't have the chance to bleed into a subsequent main.
        /// </summary>
        [JsonValueConverter]
        public string RelevantSummaryInformationFromMostRecentStoryContext { get; set; }

        [JsonValueConverter]
        public List<SummaryEntryDbModel> ShortTermSummaries { get; set; }

        [JsonValueConverter]
        public List<SummaryEntryDbModel> MediumTermSummaries { get; set; }

        [JsonValueConverter]
        public List<SummaryEntryDbModel> LongTermSummaries { get; set; }

        [JsonValueConverter]
        public List<SummaryEntryDbModel> ExtraTermSummaries { get; set; }

        [JsonValueConverter]
        public List<SummaryEntryDbModel> OverflowTermSummaries { get; set; }
    }
}
