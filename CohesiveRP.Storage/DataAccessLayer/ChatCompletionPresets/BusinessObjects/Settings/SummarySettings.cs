using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings
{
    public class SummarySettings
    {
        [JsonPropertyName("short")]
        public SummaryElementSettings Short { get; set; } = new();
    }
}
