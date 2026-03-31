using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects
{
    public class EyesDirection
    {
        [JsonPropertyName("lookingAtCharacter")]
        public string LookingAtCharacterName { get; set; }

        [JsonPropertyName("bodyPartBeingLookedAt")]
        public string BodyPartBeingLookedAt { get; set; }
    }
}
