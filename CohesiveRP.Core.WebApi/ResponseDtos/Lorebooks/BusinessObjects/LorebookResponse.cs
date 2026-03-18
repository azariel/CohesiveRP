using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects
{
    public class LorebookResponse
    {
        [JsonPropertyName("lorebookId")]
        public string LorebookId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("lastActivityAtUtc")]
        public DateTime LastActivityAtUtc { get; set; }

        [JsonPropertyName("entries")]
        public List<LorebookEntry> Entries { get; set; }
    }
}
