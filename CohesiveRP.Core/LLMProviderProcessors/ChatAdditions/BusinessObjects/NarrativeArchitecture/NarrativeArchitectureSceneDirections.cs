using System.Text.Json.Serialization;

namespace CohesiveRP.Core.LLMProviderProcessors.ChatAdditions.BusinessObjects.NarrativeArchitecture
{
    public class NarrativeArchitectureSceneDirections
    {
        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        [JsonPropertyName("fulfilled")]
        public bool Fulfilled { get; set; }
    }
}
