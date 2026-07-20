using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheets.BusinessObjects;

namespace CohesiveRP.Core.LLMProviderProcessors.Pathfinder.CharactersMutations.BusinessObjects
{
    public class CharacterStatusUpdateLLMResponse
    {
        [JsonPropertyName("characterUpdates")]
        public List<CharacterStatusUpdateEntry> CharacterUpdates { get; set; }
    }

    public class CharacterStatusUpdateEntry
    {
        [JsonPropertyName("characterName")]
        public string CharacterName { get; set; }

        [JsonPropertyName("magicalEffectsToAdd")]
        public CharacterStatusEffect[] MagicalEffectsToAdd { get; set; }

        [JsonPropertyName("magicalEffectsToRemove")]
        public string[] MagicalEffectsToRemove { get; set; }

        [JsonPropertyName("bodyStatusToAdd")]
        public CharacterStatusEffect[] BodyStatusToAdd { get; set; }

        [JsonPropertyName("bodyStatusToRemove")]
        public string[] BodyStatusToRemove { get; set; }

        [JsonPropertyName("woundsToAdd")]
        public CharacterStatusEffect[] WoundsToAdd { get; set; }

        [JsonPropertyName("woundsToRemove")]
        public string[] WoundsToRemove { get; set; }

        [JsonPropertyName("goalsForNextYearToAdd")]
        public string[] GoalsForNextYearToAdd { get; set; }

        [JsonPropertyName("goalsForNextYearToRemove")]
        public string[] GoalsForNextYearToRemove { get; set; }

        [JsonPropertyName("profession")]
        public string Profession { get; set; }

        [JsonPropertyName("relationshipsToAdd")]
        public string[] RelationshipsToAdd { get; set; }

        [JsonPropertyName("relationshipsToRemove")]
        public string[] RelationshipsToRemove { get; set; }
    }
}