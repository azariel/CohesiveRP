using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer.BusinessObjects.CharacterAnalyze
{
    public class PlayerAnalyze
    {
        [JsonPropertyName("eyesDirection")]
        public EyesDirection EyesDirection { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Analyze the mood of the characters in the scene according to their personality and Pathfinder skill checks.
        /// </summary>
        [JsonPropertyName("mood")]
        public string Mood { get; set; }

        [JsonPropertyName("facialExpression")]
        public string FacialExpression { get; set; }

        [JsonPropertyName("clothingStateOfDress")]
        public string StateOfDress { get; set; }

        [JsonPropertyName("semenOnBodyLocation")]
        public string SemenOnBodyLocation { get; set; }

        [JsonPropertyName("bodyPosition")]
        public string BodyPosition { get; set; }

        [JsonPropertyName("likelyNextThreeActions")]
        public string[] LikelyNextThreeActions { get; set; }
    }
}
