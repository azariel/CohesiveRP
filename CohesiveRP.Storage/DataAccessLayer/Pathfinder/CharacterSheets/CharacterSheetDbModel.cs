using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Chats
{
    /// <summary>
    /// Represents the structure of a normalized character sheet within the storage.
    /// </summary>
    [Table("Pathfinder_CharacterSheets")]
    public class CharacterSheetDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string CharacterSheetId { get; set; }

        [MaxLength(32)]
        public string CharacterId { get; set; }

        [MaxLength(32)]
        public string PersonaId { get; set; }

        public DateTime LastActivityAtUtc { get; set; }

        [JsonValueConverter]
        public CharacterSheet CharacterSheet { get; set; }
    }
}
