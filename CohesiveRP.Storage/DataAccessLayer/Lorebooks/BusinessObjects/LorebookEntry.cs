using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects
{
    public class LorebookEntry
    {
        [JsonPropertyName("keys")]
        public List<string> Keys { get; init; } = [];

        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; init; }

        [JsonPropertyName("insertionOrder")]
        public int InsertionOrder { get; init; }

        [JsonPropertyName("useRegex")]
        public bool UseRegex { get; init; }

        [JsonPropertyName("constant")]
        public bool Constant { get; init; }

        // -- Fields that are not supported by all formats --
        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        [JsonPropertyName("caseSensitive")]
        public bool CaseSensitive { get; set; }

        // -- Totally custom fields, owned by CohesiveRP --
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;
    }
}
