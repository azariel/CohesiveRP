using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.Summary
{
    public class SummarySettings
    {
        [JsonPropertyName("shortConfig")]
        public ShortSummaryConfig Short { get; set; } = new();

        [JsonPropertyName("mediumConfig")]
        public ExtensibleSummaryConfig Medium { get; set; } = new();

        [JsonPropertyName("longConfig")]
        public ExtensibleSummaryConfig Long { get; set; } = new();

        [JsonPropertyName("extraConfig")]
        public ExtensibleSummaryConfig Extra { get; set; } = new();

        [JsonPropertyName("overflowConfig")]
        public OverflowSummaryConfig Overflow { get; set; } = new();

        [JsonPropertyName("nbRawMessagesToKeepInContext")]
        public int NbRawMessagesToKeepInContext { get; set; } = 3;
    }
}
