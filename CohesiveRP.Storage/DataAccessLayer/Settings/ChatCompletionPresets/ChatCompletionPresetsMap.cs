using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets
{
    public class ChatCompletionPresetsMap
    {
        [JsonPropertyName("map")]
        public List<ChatCompletionPresetsMapElement> Map { get; set; }
    }
}
