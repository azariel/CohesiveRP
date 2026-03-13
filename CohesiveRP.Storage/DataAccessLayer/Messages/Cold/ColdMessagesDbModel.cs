using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Messages.Hot
{
    /// <summary>
    /// Represents the structure of old messages tied to a specific chat.
    /// </summary>
    [Table("ColdMessages")]
    public class ColdMessagesDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string ChatId { get; set; }

        [Required]
        [MaxLength(32)]
        public string CharacterId { get; set; }// The character that is at the origin of this message

        //[MaxLength(256)]
        [JsonValueConverter]
        public List<MessageDbModel> SerializedMessages { get; set; }
    }
}
