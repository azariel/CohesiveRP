using System.Text.Json.Serialization;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets
{
    public class ChatCompletionPresetsMapElement
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("type")]
        public ChatCompletionPresetType Type { get; set; }

        [JsonPropertyName("chatCompletionPresetId")]
        public string ChatCompletionPresetId { get; set; }

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; } = false;
    }
}
