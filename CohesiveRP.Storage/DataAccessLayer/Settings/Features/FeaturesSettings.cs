using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.Features
{
    public class FeaturesSettings
    {
        [JsonPropertyName("enableRelevantSummariesFeature")]
        public bool EnableRelevantSummariesFeature { get; set; }
    }
}
