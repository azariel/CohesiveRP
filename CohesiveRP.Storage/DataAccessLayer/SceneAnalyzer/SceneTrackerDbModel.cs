using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    /// <summary>
    /// Represents the structure of scene analyzers in db.
    /// </summary>
    [Table("SceneAnalyzers")]
    public class SceneAnalyzerDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string SceneAnalyzerId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }

        [MaxLength(32)]
        public string LinkedMessageId { get; set; }
    }
}
