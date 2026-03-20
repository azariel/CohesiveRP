using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.QueryModels.Chat
{
    public class CreateChatQueryModel
    {
        [JsonPropertyName("selectedChatCompletionPresets")]
        public List<ChatCompletionPresetSelection> SelectedChatCompletionPresets { get; set; }
        
        [JsonPropertyName("characterIds")]
        public List<string> CharacterIds { get; set; }

        [JsonPropertyName("lorebookIds")]
        public List<string> LorebookIds { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
