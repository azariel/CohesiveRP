using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.DataAccessLayer.Settings.Summary;
using CohesiveRP.Storage.JsonConverters;
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

        [JsonValueConverter]
        public SummarySettings Summary { get; set; }

        [JsonValueConverter]
        public List<LLMProviderConfig> LLMProviders { get; set; }

        [JsonValueConverter]
        public ChatCompletionPresetsMap ChatCompletionPresetsMap { get; set; }
    }
}
