using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format
{
    public class PromptContextFormatElement
    {
        [JsonPropertyName("tag")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PromptContextFormatTag Tag { get; set; }

        [JsonPropertyName("options")]
        public PromptContextFormatElementOptions Options { get; set; }
    }
}
