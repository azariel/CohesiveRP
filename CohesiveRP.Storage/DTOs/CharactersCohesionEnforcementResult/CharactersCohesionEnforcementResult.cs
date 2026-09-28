using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;
using CohesiveRP.Storage.DTOs.CharactersCohesionEnforcementResult;

namespace CohesiveRP.Storage.DTOs.CohesionEnforcement
{
    public class CharactersCohesionEnforcementResult
    {
        [JsonSchemaRequired]
        [JsonPropertyName("settled")]
        public List<string> Settled { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("characters")]
        public List<CharactersCohesionEnforcementCharacterDecision> Characters { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("skill_checks")]
        public string SkillChecks { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("do_not")]
        public List<string> DoNot { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("stop_point")]
        public string StopPoint { get; set; }
    }
}
