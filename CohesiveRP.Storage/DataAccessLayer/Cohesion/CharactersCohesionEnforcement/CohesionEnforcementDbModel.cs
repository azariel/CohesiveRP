using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement
{
    /// <summary>
    /// Represents the structure of Characters Cohesion Enforcement in db.
    /// </summary>
    [Table("CharactersCohesionEnforcements")]
    public class CharactersCohesionEnforcementDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string CharactersCohesionEnforcementId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }
        
        [JsonValueConverter]
        public CharactersCohesionEnforcementElement Content { get; set; }
    }
}
