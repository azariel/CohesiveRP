using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Characters
{
    public class CharacterResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("character")]
        public CharacterResponse Character {get;set; }
    }
}
