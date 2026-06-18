using System.Text.Json.Serialization;

namespace CohesiveRP.Core.LLMProviderProcessors.ChatAdditions.BusinessObjects.NarrativeArchitecture
{
    public class NarrativeArchitectureOverarchingArc
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("protagonistArc")]
        public string ProtagonistArc { get; set; }

        [JsonPropertyName("completed")]
        public bool Completed { get; set; }
    }
}
