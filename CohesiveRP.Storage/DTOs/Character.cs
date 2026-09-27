using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects;

namespace CohesiveRP.Storage.DTOs
{
    public class Character
    {
        [JsonSchemaRequired]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("creator")]
        public string Creator { get; set; }

        [JsonPropertyName("creatorNotes")]
        public string CreatorNotes { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("firstMessage")]
        public string FirstMessage { get; set; }

        [JsonPropertyName("alternateGreetings")]
        public List<string> AlternateGreetings { get; set; }

        [JsonPropertyName("imageGenerationConfiguration")]
        public CharacterImageGenerationConfiguration ImageGenerationConfiguration { get; set; }
    }
}
