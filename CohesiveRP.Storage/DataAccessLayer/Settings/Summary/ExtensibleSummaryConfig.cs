using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.Summary
{
    public class ExtensibleSummaryConfig
    {
        [JsonPropertyName("summarizeLastXTokens")]
        public int SummarizeLastXTokens { get; set; } = 256;

        [JsonPropertyName("maxTotalSummariesTokens")]
        public int MaxTotalSummariesTokens { get; set; } = 1024;
    }
}
