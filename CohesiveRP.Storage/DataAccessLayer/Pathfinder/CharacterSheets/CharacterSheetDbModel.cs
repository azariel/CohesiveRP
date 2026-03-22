using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Chats
{
    /// <summary>
    /// Represents the structure of a normalized character sheet within the storage.
    /// </summary>
    [Table("CharacterSheets")]
    public class CharacterSheetDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string CharacterId { get; set; }

        public DateTime LastActivityAtUtc { get; set; }

        [MaxLength(16384)]
        public string CharacterSheet { get; set; }
    }
}
