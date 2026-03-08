using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.Summary
{
    public class OverflowSummaryConfig
    {
        [JsonPropertyName("summarizeLastXTokens")]
        public int SummarizeLastXTokens { get; set; } = 256;

        // Contrary to the others, this is a guideline to get the AI to generate a final summary for our oldest information about this size
        // We may want to regenerate a summary until we get under this size if the AI is being stubborn as to avoid our overflow summary content to take an obscene amount of tokens
        [JsonPropertyName("maxOverflowSummaryTokens")]
        public int MaxOverflowSummaryTokens { get; set; } = 1024;
    }
}
