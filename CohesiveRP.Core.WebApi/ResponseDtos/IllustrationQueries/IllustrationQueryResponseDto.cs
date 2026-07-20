using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries
{
    public class IllustrationQueryResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("query")]
        public IllustrationQueryResponse Query { get; set; }
    }
}
