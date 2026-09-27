using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.Features
{
    public class FeaturesSettings
    {
        [JsonPropertyName("enableRelevantSummariesFeature")]
        public bool EnableRelevantSummariesFeature { get; set; }
        public bool EnableComfyUI { get; set; } = false;
    }
}
