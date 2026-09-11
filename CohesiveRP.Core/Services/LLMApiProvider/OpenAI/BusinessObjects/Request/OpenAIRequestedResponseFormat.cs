using System.Text.Json;
using System.Text.Json.Serialization;

namespace CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request
{
    public class OpenAIRequestedResponseFormat
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "json_schema";

        [JsonPropertyName("json_schema")]
        public OpenAIJsonSchema JsonSchema { get; set; }
    }

    public class OpenAIJsonSchema
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("strict")]
        public bool Strict { get; set; } = true;

        // Raw schema document — keep as JsonElement so each builder can
        // hand in whatever shape it wants without a shared C# schema model.
        [JsonPropertyName("schema")]
        public JsonElement Schema { get; set; }
    }
}