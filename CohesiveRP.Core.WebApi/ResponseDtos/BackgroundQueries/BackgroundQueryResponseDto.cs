using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.BackgroundQueries
{
    public class BackgroundQueryResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("linkedId")]
        public string LinkedId { get; set; }

        [JsonPropertyName("backgroundQueryId")]
        public string BackgroundQueryId { get; set; }

        [JsonPropertyName("dependenciesTags")]
        public List<string> DependenciesTags { get; set; }

        [JsonPropertyName("status")]
        public BackgroundQueryStatus Status { get; set; }

        [JsonPropertyName("priority")]
        public BackgroundQueryPriority Priority { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
    }
}
