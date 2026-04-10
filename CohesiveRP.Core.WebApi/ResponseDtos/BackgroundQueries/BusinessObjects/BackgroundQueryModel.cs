using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.BackgroundQueries.BusinessObjects
{
    public class BackgroundQueryModel
    {
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

        [JsonPropertyName("startFocusedGenerationDateTimeUtc")]
        public DateTime StartFocusedGenerationDateTimeUtc { get; set; }

        [JsonPropertyName("endFocusedGenerationDateTimeUtc")]
        public DateTime EndFocusedGenerationDateTimeUtc { get; set; }
    }
}
