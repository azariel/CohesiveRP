using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Core.WebApi.RequestDtos.Characters
{
    public class GetCharacterSheetResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }

        [JsonPropertyName("characterSheet")]
        public CharacterSheet CharacterSheet { get; set; }

        [JsonPropertyName("lastActivityAtUtc")]
        public DateTime LastActivityAtUtc { get; set; }
    }
}
