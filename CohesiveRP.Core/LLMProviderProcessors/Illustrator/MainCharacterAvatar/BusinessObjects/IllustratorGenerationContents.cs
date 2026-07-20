using System.Text.Json.Serialization;

namespace CohesiveRP.Core.LLMProviderProcessors.Illustrator.MainCharacterAvatar.BusinessObjects
{
    public class IllustratorGenerationContents
    {
        [JsonPropertyName("contents")]
        public List<IllustratorGenerationContent> Contents { get; set; }
    }
}
