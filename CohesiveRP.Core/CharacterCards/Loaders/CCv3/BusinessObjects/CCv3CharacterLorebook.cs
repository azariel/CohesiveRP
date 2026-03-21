using System.Text.Json.Serialization;
using CohesiveRP.Core.Lorebooks;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects
{
    public record CCv3CharacterLorebook : ILorebook
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("entries")]
        public List<CCv3CharacterLorebookEntry> Entries { get; init; } = [];
    }
}
