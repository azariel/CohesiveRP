using System.Text.Json.Serialization;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheets.BusinessObjects
{
    public class PathfinderSkillAttributes
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("skillType")]
        public PathfinderSkills SkillType { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
