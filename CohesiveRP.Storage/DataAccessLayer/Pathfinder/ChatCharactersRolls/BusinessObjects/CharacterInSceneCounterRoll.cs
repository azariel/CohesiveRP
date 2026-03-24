using System.Text.Json.Serialization;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects
{
    public class CharacterInSceneCounterRoll
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("attribute")]
        public PathfinderAttributes Attribute { get; set; }

        [JsonPropertyName("value")]
        public int Value{ get; set; }
    }
}
