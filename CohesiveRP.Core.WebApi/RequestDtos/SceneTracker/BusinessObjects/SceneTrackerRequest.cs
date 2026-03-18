using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas.BusinessObjects
{
    public class SceneTrackerRequest
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
