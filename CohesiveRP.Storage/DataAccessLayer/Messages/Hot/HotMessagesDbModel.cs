using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Messages.Hot
{
    /// <summary>
    /// Represents the structure of recent messages tied to a specific chat.
    /// </summary>
    [Table("HotMessages")]
    public class HotMessagesDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [Key]// Partition key AND FK
        public string ChatId { get; set; }

        //[MaxLength(256)]
        // Pretty much using it as NoSQL for performance. Sqlite is good, but not enterprise level. This circumvent most of the performance issues
        public string SerializedMessages { get; set; }
    }
}
