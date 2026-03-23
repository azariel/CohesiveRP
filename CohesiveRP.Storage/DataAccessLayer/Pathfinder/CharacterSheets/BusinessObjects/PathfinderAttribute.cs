using System.Text.Json.Serialization;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects
{
    public class PathfinderAttribute
    {
        [JsonPropertyName("attributeType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PathfinderAttributes AttributeType { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
