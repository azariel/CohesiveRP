using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects
{
    public class BasicInformationSceneTracker
    {
        [JsonPropertyName("mainThemes")]
        public string MainThemes { get; set; }

        [JsonPropertyName("nestedThemes")]
        public string NestedThemes { get; set; }

        [JsonPropertyName("currentDateTime")]
        public string CurrentDateTime { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }
    }
}
