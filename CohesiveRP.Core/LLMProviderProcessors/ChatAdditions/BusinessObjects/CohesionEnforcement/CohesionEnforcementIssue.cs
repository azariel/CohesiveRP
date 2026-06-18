using System.Text.Json.Serialization;

namespace CohesiveRP.Core.LLMProviderProcessors.ChatAdditions.BusinessObjects.CohesionEnforcement
{
    public class CohesionEnforcementIssue
    {
        [JsonPropertyName("severity")]
        public string Severity { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("suggestion")]
        public string Suggestion { get; set; }
    }
}
