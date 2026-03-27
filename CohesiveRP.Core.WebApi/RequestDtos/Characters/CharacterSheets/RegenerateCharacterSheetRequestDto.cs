using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.RequestDtos.Characters
{
    public class RegenerateCharacterSheetRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }

        [JsonPropertyName("characterSheetId")]
        public string CharacterSheetId { get; set; }
    }
}
