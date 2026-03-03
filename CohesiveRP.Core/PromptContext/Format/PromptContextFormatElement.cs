using System.Text.Json.Serialization;

namespace CohesiveRP.Core.PromptContext.Format
{
    public class PromptContextFormatElement
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PromptContextFormatTag Tag { get; set; }
        public PromptContextFormatElementOptions Options { get; set; }
    }
}
