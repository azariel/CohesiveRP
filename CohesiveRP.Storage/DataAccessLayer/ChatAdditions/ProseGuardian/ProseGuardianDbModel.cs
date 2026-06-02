using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.ProseGuardian.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.ProseGuardian
{
    /// <summary>
    /// Represents the structure of ProseGuardians in db.
    /// </summary>
    [Table("ProseGuardians")]
    public class ProseGuardianDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string ProseGuardianId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }
        
        [JsonValueConverter]
        public ProseGuardianElement Content { get; set; }
    }
}
