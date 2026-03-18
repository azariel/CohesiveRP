using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Chats
{
    /// <summary>
    /// Represents the structure of a lorebook within the storage.
    /// </summary>
    [Table("Lorebooks")]
    public class LorebookDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string LorebookId { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        public DateTime LastActivityAtUtc { get; set; }

        [JsonValueConverter]
        public List<LorebookEntry> Entries { get; set; } = new();
    }
}
