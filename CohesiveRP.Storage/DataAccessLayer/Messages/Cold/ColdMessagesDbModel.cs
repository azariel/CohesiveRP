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

        //[MaxLength(256)]
        [JsonValueConverter]
        public List<MessageDbModel> Messages { get; set; }
    }
}
