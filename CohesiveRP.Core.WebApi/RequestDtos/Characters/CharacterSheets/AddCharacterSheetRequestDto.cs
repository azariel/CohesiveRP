using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Core.WebApi.RequestDtos.Characters
{
    public class AddCharacterSheetRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }

        [JsonPropertyName("characterSheetId")]
        public string CharacterSheetId { get; set; }

        [JsonPropertyName("characterSheet")]
        public CharacterSheet CharacterSheet { get; set; }
    }
}
