using System.Text.Json.Serialization;

namespace CohesiveRP.Core.LLMProviderProcessors.ChatAdditions.BusinessObjects.CohesionEnforcement
{
    public class CohesionEnforcementResult
    {
        [JsonPropertyName("issues")]
        public List<CohesionEnforcementIssue> Issues { get; set; }

        [JsonPropertyName("verdict")]
        public string Verdict { get; set; }
    }
}
