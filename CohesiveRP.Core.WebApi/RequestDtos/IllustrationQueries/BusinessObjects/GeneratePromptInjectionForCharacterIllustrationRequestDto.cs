using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;

namespace CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries.BusinessObjects
{
    public class GeneratePromptInjectionForCharacterIllustrationRequestDto
    {
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("outfit")]
        public ClothingStateOfDress Outfit { get; set; }

        [JsonPropertyName("type")]
        public IllustratorQueryType Type { get; set; }
    }
}
