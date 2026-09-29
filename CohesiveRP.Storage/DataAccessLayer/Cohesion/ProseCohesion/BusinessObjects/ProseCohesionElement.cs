using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.ProseCohesion.BusinessObjects
{
    public class ProseCohesionElement
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
