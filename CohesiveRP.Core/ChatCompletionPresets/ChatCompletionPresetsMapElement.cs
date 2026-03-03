using System.Text.Json.Serialization;

namespace CohesiveRP.Core.ChatCompletionPresets
{
    public class ChatCompletionPresetsMapElement
    {
        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("chatCompletionPresetId")]
        public string ChatCompletionPresetId { get; set; }
    }
}
