using System.Text.Json.Serialization;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects
{
    public record CCv3Lorebook
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("entries")]
        public List<CCv3LorebookEntry> Entries { get; init; } = [];
    }
}
