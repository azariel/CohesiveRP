using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.RequestDtos.InteractiveUserInputQueries.BusinessObjects
{
    public class PutInteractiveUserInputQueryRequest
    {
        [JsonPropertyName("queryId")]
        public string Id { get; set; }

        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("sceneTrackerId")]
        public string SceneTrackerId { get; set; }

        [JsonPropertyName("metadata")]
        public string Metadata { get; set; }

        [JsonPropertyName("type")]
        public InteractiveUserInputType Type { get; set; }

        [JsonPropertyName("status")]
        public InteractiveUserInputStatus Status { get; set; }

        [JsonPropertyName("userChoice")]
        public bool UserChoice { get; set; }
    }
}
