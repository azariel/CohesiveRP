using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Personas
{
    public class PersonaResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("persona")]
        public PersonaResponse Persona {get;set; }
    }
}
