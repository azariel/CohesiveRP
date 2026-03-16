using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas.BusinessObjects
{
    public class PersonaRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }
    }
}
