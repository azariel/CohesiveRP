using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;

namespace CohesiveRP.Common.BusinessObjects
{
    public enum FindReplaceAction
    {
        Undefined = 0,
        Delete = 1,
        Replace = 2,
        Trim = 3,
    }

    public class FindReplace
    {
        [JsonSchemaRequired]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("action")]
        public FindReplaceAction Action { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
