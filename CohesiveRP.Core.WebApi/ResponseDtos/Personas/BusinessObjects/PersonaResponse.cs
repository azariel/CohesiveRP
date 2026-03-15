using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects
{
    public class PersonaResponse
    {
        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("lastActivityAtUtc")]
        public DateTime LastActivityAtUtc { get; set; }
    }
}
