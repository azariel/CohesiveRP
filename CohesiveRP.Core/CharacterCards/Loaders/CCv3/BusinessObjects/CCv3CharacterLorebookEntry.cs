using System.Text.Json.Serialization;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects
{
    public record CCv3CharacterLorebookEntry
    {
        [JsonPropertyName("keys")]
        public List<string> Keys { get; init; } = [];

        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; init; }

        [JsonPropertyName("insertion_order")]
        public int InsertionOrder { get; init; }

        [JsonPropertyName("use_regex")]
        public bool UseRegex { get; init; }

        [JsonPropertyName("constant")]
        public bool Constant { get; init; }
    }
}
