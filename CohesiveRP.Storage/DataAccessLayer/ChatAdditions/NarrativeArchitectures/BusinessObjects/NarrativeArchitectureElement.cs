using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeArchitecture.BusinessObjects
{
    public class NarrativeArchitectureElement
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
