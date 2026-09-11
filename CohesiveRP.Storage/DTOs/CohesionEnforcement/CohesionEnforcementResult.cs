using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;

namespace CohesiveRP.Storage.DTOs.CohesionEnforcement
{
    public class CohesionEnforcementResult
    {
        [JsonSchemaRequired]
        [JsonPropertyName("issues")]
        public List<CohesionEnforcementIssue> Issues { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("verdict")]
        public string Verdict { get; set; }
    }
}
