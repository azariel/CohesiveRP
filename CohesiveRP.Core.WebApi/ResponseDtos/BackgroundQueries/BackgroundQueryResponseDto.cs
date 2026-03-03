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
        public string ChatId { get; set; }

        [JsonPropertyName("linkedMessageId")]
        public string LinkedMessageId { get; set; }

        [JsonPropertyName("backgroundQueryId")]
        public string BackgroundQueryId { get; set; }

        [JsonPropertyName("dependenciesTags")]
        public string DependenciesTags { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("tags")]
        public string Tags { get; set; }
    }
}
