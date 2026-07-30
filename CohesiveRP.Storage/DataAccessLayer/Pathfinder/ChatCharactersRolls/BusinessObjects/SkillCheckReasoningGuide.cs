using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects
{
    public class SkillCheckReasoningGuide
    {
        [JsonPropertyName("reasoning")]
        public string Reasoning { get; set; }

        [JsonPropertyName("reactionFromOtherCharactersWhenFailingSkillCheck")]
        public string ReactionFromOtherCharactersWhenFailingSkillCheck { get; set; }

        [JsonPropertyName("reactionFromOtherCharactersWhenSucceedingSkillCheck")]
        public string ReactionFromOtherCharactersWhenSucceedingSkillCheck { get; set; }
    }
}
