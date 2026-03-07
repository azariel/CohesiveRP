using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings
{
    public class SummaryElementSettings
    {
        [JsonPropertyName("nbMessageInChunk")]
        public int NbMessageInChunk { get; set; } = 5;
    }
}
