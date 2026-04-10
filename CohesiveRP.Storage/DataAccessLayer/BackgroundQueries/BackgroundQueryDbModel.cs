using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.JsonConverters;
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
        [JsonValueConverter]
        public List<string> Tags { get; set; }

        [MaxLength(512)]
        [JsonValueConverter]
        public List<string> DependenciesTags { get; set; }

        [MaxLength(50)]
        public BackgroundQueryStatus Status { get; set; }

        public BackgroundQueryPriority Priority { get; set; }

        [MaxLength(16384)]
        public string Content { get; set; }

        [MaxLength(32)]
        public string ChatId { get; set; }

        [MaxLength(32)]
        public string LinkedId { get; set; }

        public int RetryCount { get; set; }

        /// <summary>
        /// When the background query is actually running instead of waiting on another query to finish or waiting in a queue.
        /// </summary>
        public DateTime StartFocusedGenerationDateTimeUtc { get; set; }

         /// <summary>
        /// When the background query status pass to completed or error.
        /// </summary>
        public DateTime EndFocusedGenerationDateTimeUtc { get; set; }
    }
}
