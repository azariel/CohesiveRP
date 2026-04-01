using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format
{
    public class PromptContextFormatElement
    {
        [JsonPropertyName("tag")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PromptContextFormatTag Tag { get; set; } = PromptContextFormatTag.Directive;

        [JsonPropertyName("options")]
        public PromptContextFormatElementOptions Options { get; set; } = null;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } =  false;

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
