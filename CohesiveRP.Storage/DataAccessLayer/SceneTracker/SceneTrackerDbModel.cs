using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    /// <summary>
    /// Represents the structure of scene trackers in db.
    /// </summary>
    [Table("SceneTrackers")]
    public class SceneTrackerDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string SceneTrackerId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }

        public string LinkMessageId {get;set; }
        public string Content {get;set; }
    }
}
