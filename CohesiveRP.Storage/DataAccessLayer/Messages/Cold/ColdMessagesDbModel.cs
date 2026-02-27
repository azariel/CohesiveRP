using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [Key]// Partition key AND FK
        public string ChatId { get; set; }

        //[MaxLength(256)]
        public string SerializedMessages { get; set; }
    }
}
