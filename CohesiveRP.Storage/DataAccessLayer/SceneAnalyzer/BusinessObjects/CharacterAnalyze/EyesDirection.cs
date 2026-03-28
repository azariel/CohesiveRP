using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer.BusinessObjects.CharacterAnalyze
{
    public class EyesDirection
    {
        [JsonPropertyName("lookingAtCharacter")]
        public string LookingAtCharacterName { get; set; }

        [JsonPropertyName("bodyPartBeingLookedAt")]
        public string BodyPartBeingLookedAt { get; set; }
    }
}
