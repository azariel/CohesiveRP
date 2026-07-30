using System.Text.Json.Serialization;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.ChatCharacterRolls.BusinessObjects
{
    public class ChatCharacterRoll
    {
        [JsonPropertyName("actionCategory")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PathfinderSkills ActionCategory { get; set; }

        [JsonPropertyName("charactersWhoCanResist")]
        public List<string> CharactersWhoCanResist { get; set; }

        [JsonPropertyName("guides")]
        public List<SkillCheckReasoningGuide> Guides { get; set; }

        [JsonPropertyName("bonus")]
        public int Bonus { get; set; }

        // Roll value
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("charactersInSceneWithCounterRolls")]
        public List<ChatCharacterInSceneCounterRolls> CharactersInSceneWithCounterRolls { get; set; }
    }
}
