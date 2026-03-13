using System.Text.Json.Serialization;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects
{
    public record CCv3CharacterCard : ICharacterCard
    {
        [JsonPropertyName("spec")]
        public string Spec { get; init; } = string.Empty;// "ccv3"

        [JsonPropertyName("spec_version")]
        public string SpecVersion { get; init; } = string.Empty;// "3.0"

        [JsonPropertyName("data")]
        public CCv3CharacterData Data { get; init; } = new();
    }
}
