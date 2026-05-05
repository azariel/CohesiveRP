using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;

namespace CohesiveRP.Core.LLMProviderProcessors.Illustrator.MainCharacterAvatar.BusinessObjects
{
    public class IllustratorGenerationContent
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("outfit")]
        public ClothingStateOfDress Outfit { get; set; }
    }
}
