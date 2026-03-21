using System.Text.Json.Serialization;

namespace CohesiveRP.Core.PromptContext.Builders.LoreByKeywords.BusinessObjects
{
    public class TrackedLoreEntitesShareableContext
    {
        [JsonPropertyName("entries")]
        public List<LoreEntryToTrack> Entries { get; set; }
    }
}
