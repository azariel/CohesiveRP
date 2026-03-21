using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.DataAccessLayer.Settings.Summary;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class UpdateGlobalSettingsRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("llmProviders")]
        public List<LLMProviderConfig> LLMProviders { get; set; } = new();

        [JsonPropertyName("chatCompletionPresetsMap")]
        public ChatCompletionPresetsMap ChatCompletionPresetsMap { get; set; } = new();

        [JsonPropertyName("summary")]
        public SummarySettings Summary { get; set; }
    }
}
