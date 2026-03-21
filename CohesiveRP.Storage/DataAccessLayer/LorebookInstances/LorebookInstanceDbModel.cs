using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Chats
{
    /// <summary>
    /// Represents the structure of an instance of a lorebook tethered to a specific chat within the storage.
    /// </summary>
    [Table("LorebookInstances")]
    public class LorebookInstanceDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string LorebookInstanceId { get; set; }

        [Required]
        [MaxLength(32)]
        public string LorebookId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }

        [JsonValueConverter]
        public List<LorebookStateEntry> Entries { get; set; } = new();
    }
}
