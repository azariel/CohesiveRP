using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeArchitecture.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeArchitecture
{
    /// <summary>
    /// Represents the structure of NarrativeArchitectures in db.
    /// </summary>
    [Table("NarrativeArchitectures")]
    public class NarrativeArchitectureDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string NarrativeArchitectureId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }
        
        [JsonValueConverter]
        public NarrativeArchitectureElement Content { get; set; }
    }
}
