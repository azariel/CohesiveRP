using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.RequestDtos.Characters
{
    public class UpdateCharacterRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("characterName")]
        public string CharacterName { get; set; }

        [JsonPropertyName("creatorNotes")]
        public string CreatorNotes { get; set; }

        [JsonPropertyName("characterDescription")]
        public string CharacterDescription { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }

        [JsonPropertyName("creator")]
        public string Creator { get; set; }

        [JsonPropertyName("firstMessage")]
        public string FirstMessage { get; set; }

        [JsonPropertyName("alternateGreetings")]
        public string[] AlternateGreetings { get; set; }
    }
}
