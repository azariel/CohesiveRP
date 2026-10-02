using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;

namespace CohesiveRP.Common.BusinessObjects
{
    public class FindReplaceResultDto
    {
        [JsonSchemaRequired]
        [JsonPropertyName("mutations")]
        public FindReplace[] Mutations { get; set; }
    }
}
