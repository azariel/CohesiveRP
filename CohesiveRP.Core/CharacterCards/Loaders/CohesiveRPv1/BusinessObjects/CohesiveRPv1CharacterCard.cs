using System.Text.Json.Serialization;

namespace CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects
{
    public class CohesiveRPv1CharacterCard : ICharacterCard
    {
        [JsonPropertyName("spec")]
        public string Spec { get; init; } = string.Empty;// "ccv3"

        [JsonPropertyName("spec_version")]
        public string SpecVersion { get; init; } = string.Empty;// "3.0"

        [JsonPropertyName("data")]
        public CohesiveRPv1Data Data { get; init; } = new();
    }
}
