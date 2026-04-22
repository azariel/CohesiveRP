using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects
{
    public class IllustrationMapOutfit
    {
        // The actual prompt that will get injected into the illustrator (image generator) prompt context.
        /* ex:
         * score_9, masterpiece, best quality, score_8_up, score_7_up, 
           1girl, solo, adult, woman, upper body, facing viewer, looking at viewer, standing, straight-on,

           human, short, Dark with subtle blue tones hair, short hair, slightly asymmetrical hair, clean cut hair, fair skin, calm observant grey-blue eyes, large breasts, athletic and balanced build, three monthspregnant with swelling breasts and rounding stomach,
           naked, neutral expression,

           good head proportions, black background, simple background, detailed face, beautiful eyes
        */
        [JsonPropertyName("illustratorPromptInjection")]
        public string IllustratorPromptInjection { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("outfit")]
        public ClothingStateOfDress Outfit { get; set; }

        [JsonPropertyName("sourceAvatars")]
        public List<CharacterAvatar> SourceAvatars { get; set; }
    }
}
