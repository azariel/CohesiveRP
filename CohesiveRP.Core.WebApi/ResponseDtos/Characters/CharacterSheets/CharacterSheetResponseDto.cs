using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Characters.CharacterSheets
{
    public class CharacterSheetResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("characterSheet")]
        public CharacterSheet CharacterSheet {get;set; }
    }
}
