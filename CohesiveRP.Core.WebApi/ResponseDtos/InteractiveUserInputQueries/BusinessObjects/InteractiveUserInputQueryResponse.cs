using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.InteractiveUserInputQueries.BusinessObjects
{
    public class InteractiveUserInputQueryResponse
    {
        [JsonPropertyName("queryId")]
        public string Id { get; set; }

        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("sceneTrackerId")]
        public string SceneTrackerId { get; set; }

        [JsonPropertyName("createdAtUtc")]
        public DateTime? CreatedAtUtc { get; set; }

        [JsonPropertyName("metadata")]
        public string Metadata { get; set; }

        [JsonPropertyName("type")]
        public InteractiveUserInputType Type { get; set; }

        [JsonPropertyName("status")]
        public InteractiveUserInputStatus Status { get; set; }
    }
}
