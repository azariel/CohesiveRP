using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.StyleCohesion.BusinessObjects
{
    public class StyleCohesionElement
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
