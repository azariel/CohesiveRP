using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;

namespace CohesiveRP.Core.PromptContext.Builders.LoreByKeywords.BusinessObjects
{
    public class TrackableLorebookEntry
    {
        [JsonPropertyName("lorebookId")]
        public string LorebookId {get;set; }

        [JsonPropertyName("linkedMessageId")]
        public string LinkedMessageId {get;set; }

        [JsonPropertyName("entry")]
        public LorebookEntry Entry {get;set; }
    }
}
