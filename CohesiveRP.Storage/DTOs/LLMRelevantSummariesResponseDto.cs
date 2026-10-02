using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;

namespace CohesiveRP.Storage.DTOs
{
    public class LLMRelevantSummariesResponseDto
    {
        [JsonSchemaRequired]
        [JsonPropertyName("shortTermSummaries")]
        public string ShortTermSummaries { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("mediumTermSummaries")]
        public string MediumTermSummaries { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("longTermSummaries")]
        public string LongTermSummaries { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("veryLongTermSummaries")]
        public string VeryLongTermSummaries { get; set; }
    }
}
