using System.Text.Json.Serialization;

namespace CohesiveRP.Common.BusinessObjects
{
    public class StringResultWrapper
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
