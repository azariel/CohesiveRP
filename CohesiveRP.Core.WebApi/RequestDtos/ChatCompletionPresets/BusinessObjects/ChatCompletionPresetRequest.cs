using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.WebApi.RequestDtos.ChatCompletionPresets.BusinessObjects
{
    public class ChatCompletionPresetRequest
    {
        [JsonPropertyName("name")]
        public string Name {get;set; }

        [JsonPropertyName("format")]
        public GlobalPromptContextFormat Format {get;set; }
    }
}
