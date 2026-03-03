using System.Text.Json.Serialization;

namespace CohesiveRP.Core.ChatCompletionPresets
{
    public class ChatCompletionPresetsMap
    {
        [JsonPropertyName("map")]
        public List<ChatCompletionPresetsMapElement> Map { get; set; }
    }
}
