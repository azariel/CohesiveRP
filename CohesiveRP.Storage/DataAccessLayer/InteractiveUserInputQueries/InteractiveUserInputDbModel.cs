using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries
{
    /// <summary>
    /// Represents the structure of queries made to the User for input during an interactive process. This table is designed to store and manage the state of user input queries, including their status and associated metadata.
    /// </summary>
    [Table("InteractiveUserInputQueries")]
    public class InteractiveUserInputDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string InteractiveUserInputQueryId { get; set; }

        [Required]
        [MaxLength(32)]// FK
        public string ChatId { get; set; }

        [MaxLength(32)]// FK
        public string SceneTrackerId { get; set; }

        [Required]
        [MaxLength(64)]
        public InteractiveUserInputType Type { get; set; }

        [Required]
        [MaxLength(64)]
        public InteractiveUserInputStatus Status { get; set; }

        [MaxLength(8196)]
        public string Metadata { get; set; }

        public bool? UserChoice { get; set; }
    }
}
