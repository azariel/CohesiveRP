using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.ProseCohesion.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement
{
    /// <summary>
    /// Represents the structure of ProseCohesion in db.
    /// </summary>
    [Table("ProseCohesions")]
    public class ProseCohesionDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string ProseCohesionId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }
        
        [JsonValueConverter]
        public ProseCohesionElement Content { get; set; }
    }
}
