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
    }
}
