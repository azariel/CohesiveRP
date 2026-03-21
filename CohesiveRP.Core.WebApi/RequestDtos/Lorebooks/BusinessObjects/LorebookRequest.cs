using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas.BusinessObjects
{
    public class LorebookRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("entries")]
        public List<LorebookEntry> Entries { get; set; }
    }
}
