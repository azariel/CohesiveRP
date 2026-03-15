using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Characters.BusinessObjects
{
    public class SceneTrackerResponse
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; internal set; }

        [JsonPropertyName("sceneTrackerId")]
        public string SceneTrackerId { get; internal set; }

        [JsonPropertyName("createdAtUtc")]
        public DateTime? CreatedAtUtc { get; internal set; }

        [JsonPropertyName("linkMessageId")]
        public string LinkMessageId { get; internal set; }
    }
}
