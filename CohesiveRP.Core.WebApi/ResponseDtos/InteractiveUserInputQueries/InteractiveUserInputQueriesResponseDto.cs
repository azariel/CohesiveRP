using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.InteractiveUserInputQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.InteractiveUserInputQueries
{
    public class InteractiveUserInputQueriesResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("queries")]
        public InteractiveUserInputQueryResponse[] Queries { get; set; }
    }
}
