using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;

namespace CohesiveRP.Common.BusinessObjects
{
    public class SceneTrackerValidationResult
    {
        [JsonSchemaRequired]
        [JsonPropertyName("recommendations")]
        public string[] Recommendations { get; set; } = [];
    }
}
