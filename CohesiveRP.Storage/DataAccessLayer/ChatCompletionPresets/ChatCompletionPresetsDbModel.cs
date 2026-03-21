using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.AIQueries
{
    /// <summary>
    /// Represents the structure of global settings within the storage.
    /// </summary>
    [Table("ChatCompletionPresets")]
    public class ChatCompletionPresetsDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string ChatCompletionPresetId { get; set; }

        //[MaxLength(1024)]
        [JsonValueConverter]
        public GlobalPromptContextFormat Format { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }
    }
}
