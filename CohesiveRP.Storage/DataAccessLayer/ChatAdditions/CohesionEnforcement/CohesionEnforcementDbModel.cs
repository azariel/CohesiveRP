using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.CohesionEnforcement.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.CohesionEnforcement
{
    /// <summary>
    /// Represents the structure of Cohesion Enforcement in db.
    /// </summary>
    [Table("CohesionEnforcements")]
    public class CohesionEnforcementDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string CohesionEnforcementId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }
        
        [JsonValueConverter]
        public CohesionEnforcementElement Content { get; set; }
    }
}
