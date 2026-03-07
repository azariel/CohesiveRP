using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Summary;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    /// <summary>
    /// Represents the structure of short term summaries in db.
    /// </summary>
    [Table("ShortTermSummaries")]
    public class ShortTermSummaryDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string SummaryId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }

        [JsonValueConverter]
        public List<SummaryDbModel> Summaries { get; set; }
    }
}
