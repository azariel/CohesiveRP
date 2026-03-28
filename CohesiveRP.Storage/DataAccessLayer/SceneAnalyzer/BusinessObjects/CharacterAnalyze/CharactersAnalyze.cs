using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer.BusinessObjects.CharacterAnalyze
{
    public class CharactersAnalyze
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Analyze the mood of the characters in the scene according to their personality and Pathfinder skill checks.
        /// </summary>
        [JsonPropertyName("mood")]
        public string Mood { get; set; }

        //[JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("facialExpression")]
        public string FacialExpression { get; set; }

        //[JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("clothingStateOfDress")]
        public string StateOfDress { get; set; }

        //[JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("semenOnBodyLocation")]
        public string SemenOnBodyLocation { get; set; }

        //[JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("bodyPosition")]
        public string BodyPosition { get; set; }

        [JsonPropertyName("innerThoughtsOrMonologue")]
        public string InnerThoughtsOrMonologue { get; set; }

        [JsonPropertyName("likelyNextThreeActions")]
        public string[] LikelyNextThreeActions { get; set; }
    }
}
