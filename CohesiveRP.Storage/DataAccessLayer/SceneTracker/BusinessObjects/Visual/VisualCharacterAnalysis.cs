using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects.Visual
{
    public class VisualCharacterAnalysis
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("facialExpression")]
        public string FacialExpression { get; set; }

        //[JsonPropertyName("outfit")]
        //public string Outfit { get; set; }

        [JsonPropertyName("clothingStateOfDress")]
        public string ClothingStateOfDress { get; set; }

        [JsonPropertyName("semenOnBodyLocation")]
        public string SemenOnBodyLocation { get; set; }

        [JsonPropertyName("bodyPosition")]
        public string BodyPosition { get; set; }
    }
}
