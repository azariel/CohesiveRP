using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects
{
    public class CharacterImageGenerationConfiguration
    {
        // Tag of the illustrator (image generator) to use for this character. Could reference ComfyUI using a specific model and a specific workflow for example.
        [JsonPropertyName("illustratorTag")]
        public string IllustratorTag { get; set; }

        [JsonPropertyName("illustrationMapOutfits")]
        public List<IllustrationMapOutfit> IllustrationMapOutfits { get; set; }
    }
}
