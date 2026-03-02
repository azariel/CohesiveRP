using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.ResponseDtos.BackgroundQueries
{
    public class BackgroundQueryResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("chatId")]
        public string ChatId { get; internal set; }

        [JsonPropertyName("backgroundQueryId")]
        public string BackgroundQueryId { get; internal set; }

        [JsonPropertyName("dependenciesTags")]
        public string DependenciesTags { get; internal set; }

        [JsonPropertyName("status")]
        public string Status { get; internal set; }

        [JsonPropertyName("priority")]
        public int Priority { get; internal set; }

        [JsonPropertyName("content")]
        public string Content { get; internal set; }

        [JsonPropertyName("tags")]
        public string Tags { get; internal set; }
    }
}
