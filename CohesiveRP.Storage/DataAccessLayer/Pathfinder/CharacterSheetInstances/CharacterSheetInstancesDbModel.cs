using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Chats
{
    /// <summary>
    /// Represents the structure of a normalized character sheet INSTANCE (for a specific chat) within the storage.
    /// </summary>
    [Table("Pathfinder_CharacterSheetInstances")]
    public class CharacterSheetInstancesDbModel : CohesiveRPSqliteBaseTable
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
        public List<CharacterSheetInstance> CharacterSheetInstances { get; set; }
    }
}
