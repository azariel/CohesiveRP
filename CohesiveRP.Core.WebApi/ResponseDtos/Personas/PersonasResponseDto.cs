using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Personas
{
    public class PersonasResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("personas")]
        public List<PersonaResponse> Personas { get; set; }
    }
}
