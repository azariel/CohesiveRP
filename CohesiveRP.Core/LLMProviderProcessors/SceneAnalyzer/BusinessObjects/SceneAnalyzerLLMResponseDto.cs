using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer.BusinessObjects.CharacterAnalyze;

namespace CohesiveRP.Core.LLMProviderProcessors.SceneAnalyzer.BusinessObjects
{
    public class SceneAnalyzerLLMResponseDto
    {
        [JsonPropertyName("charactersAnalyze")]
        public CharactersAnalyze[] CharactersAnalyze { get; set; }

        [JsonPropertyName("playerAnalyze")]
        public PlayerAnalyze PlayerAnalyze { get; set; }

        [JsonPropertyName("sceneCategory")]
        public SceneCategory SceneCategory { get; set; }
    }
}
