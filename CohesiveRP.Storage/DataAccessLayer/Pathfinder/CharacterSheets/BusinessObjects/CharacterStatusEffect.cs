using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheets.BusinessObjects
{
    public class CharacterStatusEffect
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }// e.g: "Cursed with a minor truth-compulsion" or "Deep laceration on left forearm, still bleeding"

        [JsonPropertyName("expiresAt")]
        public string ExpiresAt { get; set; }// "PERMANENT", "SEMI-PERMANENT", or an exact datetime formatted like the SceneTracker's currentDateTime, e.g. "4 October 1995 18:00:00"
    }
}