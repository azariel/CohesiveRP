using System.Text.Json.Serialization;

namespace CohesiveRP.Common.Utils.Parsers.BusinessObjects
{
    public class ApiMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
