using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeDirection.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeDirection
{
    /// <summary>
    /// Represents the structure of NarrativeDirection in db.
    /// </summary>
    [Table("NarrativeDirections")]
    public class NarrativeDirectionDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string NarrativeDirectionId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }
        
        [JsonValueConverter]
        public NarrativeDirectionElement Content { get; set; }
    }
}
