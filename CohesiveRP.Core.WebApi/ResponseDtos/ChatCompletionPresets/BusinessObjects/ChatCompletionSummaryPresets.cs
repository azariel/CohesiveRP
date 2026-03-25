using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets.BusinessObjects
{
    public class ChatCompletionSummaryPresets
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("chatCompletionPresetId")]
        public string ChatCompletionPresetId { get; set; }
    }
}
