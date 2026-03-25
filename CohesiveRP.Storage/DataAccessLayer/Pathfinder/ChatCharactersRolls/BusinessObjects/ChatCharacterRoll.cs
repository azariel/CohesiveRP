using System.Text.Json.Serialization;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects
{
    public class ChatCharacterRoll
    {
        [JsonPropertyName("actionCategory")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PathfinderSkills ActionCategory { get; set; }

        [JsonPropertyName("reasonings")]
        public List<string> Reasonings { get; set; }

        // Roll value
        [JsonPropertyName("value")]
        public int Value { get; set; }

        // Nb of turns of User remaining to inject the roll into the prompt context. <= 0 won,t get injected. Otherwise, it'll get injected for the right amount of turns
        [JsonPropertyName("nbRemainingInjectionTurns")]
        public int NbRemainingInjectionTurns { get; set; }

        [JsonPropertyName("charactersInScene")]
        public CharacterInScene[] CharactersInScene { get; set; }

        [JsonPropertyName("nbRemainingRollFreeze")]
        public int NbRemainingRollFreeze { get; set; }
    }
}
