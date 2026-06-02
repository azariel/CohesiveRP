using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.ProseGuardian.BusinessObjects
{
    public class ProseGuardianElement
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
