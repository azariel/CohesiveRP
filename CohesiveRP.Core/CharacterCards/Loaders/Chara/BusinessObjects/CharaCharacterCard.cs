using System.Text.Json.Serialization;

namespace CohesiveRP.Core.CharacterCards.Loaders.Chara.BusinessObjects
{
    public record CharaCharacterCard : ICharacterCard
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("personality")]
        public string Personality { get; set; } = string.Empty;

        [JsonPropertyName("scenario")]
        public string Scenario { get; set; } = string.Empty;

        [JsonPropertyName("first_mes")]
        public string FirstMessage { get; set; } = string.Empty;

         [JsonPropertyName("mes_example")]
        public string MessageExamples { get; set; } = string.Empty;
    }
}
