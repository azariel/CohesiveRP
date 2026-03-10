using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.Summary
{
    public class SummaryConfig
    {
        [JsonPropertyName("summarizeLastXMessages")]
        public int SummarizeLastXMessages { get; set; } = 5;

        [JsonPropertyName("maxShortTermSummaryTokens")]
        public int MaxShortTermSummaryTokens { get; set; } = 1024;
    }
}
