using System.Text.Json.Serialization;

namespace CohesiveRP.Core.LLMProviderProcessors.ChatAdditions.BusinessObjects.NarrativeArchitecture
{
    public class NarrativeArchitectureResult
    {
        [JsonPropertyName("overarchingArc")]
        public NarrativeArchitectureOverarchingArc OverarchingArc { get; set; }

        [JsonPropertyName("sceneDirections")]
        public List<NarrativeArchitectureSceneDirections> SceneDirections { get; set; }

        [JsonPropertyName("pacing")]
        public string Pacing { get; set; }

        [JsonPropertyName("staleDetected")]
        public bool StaleDetected { get; set; }
    }
}
