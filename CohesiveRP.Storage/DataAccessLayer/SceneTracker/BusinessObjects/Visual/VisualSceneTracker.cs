using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects.Visual
{
    public class VisualSceneTracker
    {
        [JsonPropertyName("charactersAnalysis")]
        public VisualCharacterAnalysis[] CharactersAnalysis { get; set; }

        [JsonPropertyName("playerAnalysis")]
        public VisualPlayerAnalysis PlayerAnalysis { get; set; }
    }
}
