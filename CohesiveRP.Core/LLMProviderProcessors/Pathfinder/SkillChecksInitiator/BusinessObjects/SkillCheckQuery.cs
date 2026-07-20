using System.Text.Json.Serialization;

namespace CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects
{
    public class SkillCheckQuery
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("actionCategory")]
        public PathfinderSkills ActionCategory { get; set; }

        [JsonPropertyName("charactersWhoCanResist")]
        public List<string> CharactersWhoCanResist { get; set; }

        [JsonPropertyName("bonus")]
        public int Bonus { get; set; }

        [JsonPropertyName("reasonings")]
        public List<string> Reasonings { get; set; }
    }
}
