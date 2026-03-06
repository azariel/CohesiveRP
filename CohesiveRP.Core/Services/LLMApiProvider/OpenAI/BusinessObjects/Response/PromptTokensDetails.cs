using System.Text.Json.Serialization;

namespace CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Response
{
    public class PromptTokensDetails
    {
        [JsonPropertyName("cached_tokens")]
        public int CachedTokens { get; set; }

        [JsonPropertyName("audio_tokens")]
        public int AudioTokens { get; set; }
    }
}
