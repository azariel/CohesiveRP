using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.CohesionEnforcement.BusinessObjects
{
    public class CharactersCohesionEnforcementElement
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
