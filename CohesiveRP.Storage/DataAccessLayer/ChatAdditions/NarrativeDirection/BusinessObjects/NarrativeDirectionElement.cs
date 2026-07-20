using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeDirection.BusinessObjects
{
    public class NarrativeDirectionElement
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
