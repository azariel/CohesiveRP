using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;

namespace CohesiveRP.Storage.DTOs.CohesionEnforcement
{
    public class CohesionEnforcementIssue
    {
        [JsonSchemaRequired]
        [JsonPropertyName("severity")]
        public string Severity { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("suggestion")]
        public string Suggestion { get; set; }
    }
}
