using System.Text.Json.Serialization;

namespace CohesiveRP.Core.Lorebooks.Loaders.CCv3.BusinessObjects
{
    public class CCv3Lorebook : ILorebook
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The JSON object key is a numeric string ("0", "1", …), so we deserialize
        /// the whole block as a dictionary and let callers iterate Values if they
        /// want an ordered list.
        /// </summary>
        [JsonPropertyName("entries")]
        public Dictionary<string, CCv3LorebookEntry> Entries { get; set; } = new();

        [JsonPropertyName("extensions")]
        public CCv3LorebookExtensions Extensions { get; set; } = new();

        [JsonPropertyName("is_creation")]
        public bool IsCreation { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("recursive_scanning")]
        public bool RecursiveScanning { get; set; }

        [JsonPropertyName("scan_depth")]
        public int ScanDepth { get; set; }

        [JsonPropertyName("token_budget")]
        public int TokenBudget { get; set; }
    }
}
