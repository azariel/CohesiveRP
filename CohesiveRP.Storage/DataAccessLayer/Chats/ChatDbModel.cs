using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Chats
{
    /// <summary>
    /// Represents the structure of a chat within the storage.
    /// </summary>
    [Table("Chats")]
    public class ChatDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string ChatId { get; set; }

        [JsonValueConverter]
        public List<ChatCompletionPresetSelection> SelectedChatCompletionPresets { get; set; }

        /// <summary>
        /// The list of characters tied to this chat. In a standard chat, it'll have a single character. In a group chat, there'll be multiple characters (+narrator?)
        /// </summary>
        [JsonValueConverter]
        public List<string> CharacterIds { get; set; }
    }
}
