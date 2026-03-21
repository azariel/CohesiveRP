using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets.BusinessObjects
{
    public class ChatCompletionAllPresets
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("chatCompletionPresetId")]
        public string ChatCompletionPresetId { get; set; }
    }
}
