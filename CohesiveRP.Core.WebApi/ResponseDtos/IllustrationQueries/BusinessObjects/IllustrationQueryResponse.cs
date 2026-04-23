using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.IllustrationQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries.BusinessObjects
{
    public class IllustrationQueryResponse
    {
        [JsonPropertyName("queryId")]
        public string Id { get; set; }

        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("createdAtUtc")]
        public DateTime? CreatedAtUtc { get; set; }

        [JsonPropertyName("type")]
        public IllustratorQueryType Type { get; set; }

        [JsonPropertyName("status")]
        public IllustratorQueryStatus Status { get; set; }
    }
}
