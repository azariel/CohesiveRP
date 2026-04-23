using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.IllustrationQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries
{
    /// <summary>
    /// Represents the structure of queries made to generate images for CohesiveRp.
    /// </summary>
    [Table("IllustrationQueries")]
    public class IllustrationQueryDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string IllustrationQueryId { get; set; }

        // Nullable
        [MaxLength(32)]// FK
        public string ChatId { get; set; }

        [MaxLength(32)]//FK
        public string CharacterId { get; set; }

        [Required]
        [MaxLength(128)]
        public IllustratorQueryType Type { get; set; }

        [Required]
        [MaxLength(128)]
        public IllustratorQueryStatus Status { get; set; }
    }
}
