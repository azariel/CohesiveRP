using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Chats
{
    /// <summary>
    /// Represents the structure of a normalized character rolls within a chat within the storage.
    /// </summary>
    [Table("Pathfinder_ChatCharactersRolls")]
    public class ChatCharactersRollsDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string ChatId { get; set; }

        public DateTime LastActivityAtUtc { get; set; }

        [JsonValueConverter]
        public List<ChatCharacterRolls> ChatCharactersRolls { get; set; }
    }
}
