using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Chats
{
    /// <summary>
    /// Represents the structure of a persona within the storage.
    /// </summary>
    [Table("Personas")]
    public class PersonaDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string PersonaId { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        [MaxLength(8196)]
        public string Description { get; set; }

        public DateTime LastActivityAtUtc { get; set; }
    }
}
