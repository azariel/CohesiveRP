using System.Text.Json.Serialization;

namespace CohesiveRP.Core.Lorebooks.Loaders.CCv3.BusinessObjects
{
    public class CCv3CharacterFilter
    {
        [JsonPropertyName("isExclude")]
        public bool IsExclude { get; set; }

        [JsonPropertyName("names")]
        public List<string> Names { get; set; } = new();

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();
    }
}
