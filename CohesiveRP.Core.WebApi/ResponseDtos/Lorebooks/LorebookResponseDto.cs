using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Personas
{
    public class LorebookResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("lorebook")]
        public LorebookResponse Lorebook {get;set; }
    }
}
