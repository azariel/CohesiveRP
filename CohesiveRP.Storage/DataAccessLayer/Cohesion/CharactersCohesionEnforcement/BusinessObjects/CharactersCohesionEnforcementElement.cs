using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement.BusinessObjects
{
    public class CharactersCohesionEnforcementElement
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
