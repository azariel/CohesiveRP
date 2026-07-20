using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.CohesionEnforcement.BusinessObjects
{
    public class CohesionEnforcementElement
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
