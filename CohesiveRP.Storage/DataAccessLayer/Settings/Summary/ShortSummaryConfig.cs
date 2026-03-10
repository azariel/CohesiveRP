using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.Summary
{
    public class ShortSummaryConfig
    {
        [JsonPropertyName("nbMessageInChunk")]
        public int NbMessageInChunk { get; set; } = 3;

        [JsonPropertyName("maxShortTermSummaryTokens")]
        public int MaxShortTermSummaryTokens { get; set; } = 1024;
    }
}
