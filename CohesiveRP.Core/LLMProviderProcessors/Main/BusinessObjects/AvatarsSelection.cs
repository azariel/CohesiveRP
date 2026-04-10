using System.Text.Json.Serialization;

namespace CohesiveRP.Core.LLMProviderProcessors.Main.BusinessObjects
{
    public class AvatarsSelection
    {
        [JsonPropertyName("charactersAvatarPath")]
        public List<string> CharactersAvatarPath { get; set; } = new();
    }
}
