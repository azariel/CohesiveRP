using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer.BusinessObjects
{
    public class SceneCategory
    {
        [JsonPropertyName("mainThemes")]
        public string MainThemes { get; set; }

        [JsonPropertyName("nestedThemes")]
        public string NestedThemes { get; set; }
    }
}
