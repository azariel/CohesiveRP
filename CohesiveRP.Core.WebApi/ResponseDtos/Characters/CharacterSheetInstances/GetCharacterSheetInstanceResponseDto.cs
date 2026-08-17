using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Core.WebApi.RequestDtos.Characters
{
    public class GetCharacterSheetInstanceResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("characterSheet")]
        public CharacterSheet CharacterSheet { get; set; }

        [JsonPropertyName("lastActivityAtUtc")]
        public DateTime LastActivityAtUtc { get; set; }

        [JsonPropertyName("characterSheetId")]
        public string CharacterSheetId { get; set; }

        [JsonPropertyName("characterSheetInstanceId")]
        public string CharacterSheetInstanceId { get; set; }
    }
}
