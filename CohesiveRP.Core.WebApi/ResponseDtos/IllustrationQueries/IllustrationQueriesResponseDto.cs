using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries
{
    public class IllustrationQueriesResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("queries")]
        public IllustrationQueryResponse[] Queries { get; set; }
    }
}
