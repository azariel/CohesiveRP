using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;

namespace CohesiveRP.Storage.DTOs.CohesionEnforcement
{
    public class CharactersCohesionEnforcementResult
    {
        [JsonSchemaRequired]
        [JsonPropertyName("issues")]
        public List<CharactersCohesionEnforcementIssue> Issues { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("verdict")]
        public string Verdict { get; set; }
    }
}
