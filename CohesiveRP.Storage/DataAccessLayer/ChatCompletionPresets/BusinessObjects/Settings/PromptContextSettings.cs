using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings
{
    public class PromptContextSettings
    {
        [JsonPropertyName("lastXMessages")]
        public int LastXMessages { get; set; } = 5;

        [JsonPropertyName("summary")]
        public SummarySettings Summary { get; set; } = new();
    }
}
