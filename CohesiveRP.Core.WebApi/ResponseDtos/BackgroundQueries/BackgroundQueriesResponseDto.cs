using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.BackgroundQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.BackgroundQueries
{
    public class BackgroundQueriesResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("queries")]
        public BackgroundQueryModel[] Queries { get; set; }
    }
}
