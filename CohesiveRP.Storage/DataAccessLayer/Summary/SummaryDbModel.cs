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
