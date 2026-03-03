using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.AIQueries
{
    /// <summary>
    /// Represents the structure of queries made to background backend within the storage.
    /// </summary>
    [Table("BackgroundQueries")]
    public class BackgroundQueryDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string BackgroundQueryId { get; set; }

        [MaxLength(512)]
        public string Tags { get; set; }

        [MaxLength(512)]
        public string DependenciesTags { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        public int Priority { get; set; }

        [MaxLength(16384)]
        public string Content { get; set; }

        [MaxLength(32)]
        public string ChatId { get; set; }

        [MaxLength(32)]
        public string LinkedMessageId { get; set; }
    }
}
