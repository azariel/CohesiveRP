using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.SamplingSettings
{
    public class OpenAISamplingSettings
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = 1.0f;

        [JsonPropertyName("min_p")]
        public float MinP { get; set; } = 0.1f;// Min-P removes only tokens that are too unlikely relative to the best token, which tends to preserve creativity while filtering obvious garbage. This makes TopP paired with TopK alsmost irrelevant.

        // DRY
        [JsonPropertyName("dry_multiplier")]
        public float DryMultiplier { get; set; } = 0.8f;

        [JsonPropertyName("dry_base")]
        public float DryBase { get; set; } = 1.75f;

        [JsonPropertyName("dry_allowed_length")]
        public int DryAllowedLength { get; set; } = 2;

        [JsonPropertyName("dry_sequence_breakers")]
        public string[] DrySequenceBreakers { get; set; } = ["\n", ":", "\"", "*"];
        // ----------

        // XTC
        [JsonPropertyName("xtc_probability")]
        public float XTCProbability { get; set; } = 0.3f;

        [JsonPropertyName("xtc_treshold")]
        public float XTCTreshold { get; set; } = 0.1f;
        // --------------

        // Older settings, most modern models leave those settings as-is
        [JsonPropertyName("top_p")]
        public float TopP { get; set; } = 1.0f;// Top-P dynamically changes how many tokens are available depending on the entropy of the distribution.

        [JsonPropertyName("top_k")]
        public float TopK { get; set; } = 0.0f;
        // --------------
    }
}
