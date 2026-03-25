using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets.BusinessObjects
{
    public class ChatCompletionPreset
    {
        [JsonPropertyName("chatCompletionPresetId")]
        public string ChatCompletionPresetId {get;set; }

        [JsonPropertyName("name")]
        public string Name {get;set; }

        [JsonPropertyName("format")]
        public GlobalPromptContextFormat Format {get;set; }
    }
}
