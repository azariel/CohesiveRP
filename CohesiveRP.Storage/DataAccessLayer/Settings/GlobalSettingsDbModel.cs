using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Settings
{
    /// <summary>
    /// Represents the structure of global settings within the storage.
    /// </summary>
    [Table("GlobalSettings")]
    public class GlobalSettingsDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string GlobalSettingsId { get; set; }

        //[MaxLength(1024)]
        public string LLMProviders { get; set; }

        public string ChatCompletionPresetsMap { get; set; }
    }
}
