using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Characters.BusinessObjects
{
    public class CharacterResponse
    {
        [JsonPropertyName("createdAtUtc")]
        public DateTime? CreatedAtUtc { get; set; }

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("creator")]
        public string Creator { get; set; }

        [JsonPropertyName("creatorNotes")]
        public string CreatorNotes { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("firstMessage")]
        public string FirstMessage { get; set; }

        [JsonPropertyName("alternateGreetings")]
        public List<string> AlternateGreetings { get; set; }

        [JsonPropertyName("lastActivityAtUtc")]
        public DateTime LastActivityAtUtc { get; set; }

        [JsonPropertyName("imageGenerationConfiguration")]
        public CharacterImageGenerationConfiguration ImageGenerationConfiguration { get; set; }
    }
}
