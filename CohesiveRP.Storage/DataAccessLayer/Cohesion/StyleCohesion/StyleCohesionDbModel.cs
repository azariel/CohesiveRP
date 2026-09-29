using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.ProseCohesion.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.StyleCohesion.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement
{
    /// <summary>
    /// Represents the structure of StyleCohesion in db.
    /// </summary>
    [Table("StyleCohesions")]
    public class StyleCohesionDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string StyleCohesionId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }
        
        [JsonValueConverter]
        public StyleCohesionElement Content { get; set; }
    }
}
