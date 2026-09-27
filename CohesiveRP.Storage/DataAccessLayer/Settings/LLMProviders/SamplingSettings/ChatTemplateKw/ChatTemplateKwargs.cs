using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.SamplingSettings.ChatTemplateKw
{
    public class ChatTemplateKwargs
    {
        [JsonPropertyName("enable_thinking")]
        public bool enableThinking { get; set; } = false;
    }
}
