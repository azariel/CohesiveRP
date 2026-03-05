using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.QueryModels.Chat
{
    public class ChatCompletionPresetSelection
    {
        [JsonPropertyName("type")]
        public ChatCompletionPresetType Type { get; set; }

        [JsonPropertyName("chatCompletionPresetId")]
        public string ChatCompletionPresetId { get; set; }
    }
}
