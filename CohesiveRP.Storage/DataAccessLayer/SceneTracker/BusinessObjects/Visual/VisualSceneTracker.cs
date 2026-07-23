using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects.Visual
{
    public class VisualSceneTracker
    {
        [JsonPropertyName("allCharactersActiveInScene")]
        public string[] AllCharacterNamesActiveInScene { get; set; }

        [JsonPropertyName("charactersAnalysis")]
        public VisualCharacterAnalysis[] CharactersAnalysis { get; set; }

        [JsonPropertyName("playerAnalysis")]
        public VisualPlayerAnalysis PlayerAnalysis { get; set; }
    }
}
