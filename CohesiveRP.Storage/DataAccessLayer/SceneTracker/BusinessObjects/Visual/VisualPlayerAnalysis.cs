using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects.Visual
{
    public class VisualPlayerAnalysis : VisualCharacterAnalysis
    {
        [JsonPropertyName("eyesDirection")]
        public EyesDirection EyesDirection { get; set; }
    }
}
