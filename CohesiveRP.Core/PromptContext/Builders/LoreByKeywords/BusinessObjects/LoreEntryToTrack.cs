namespace CohesiveRP.Core.PromptContext.Builders.LoreByKeywords.BusinessObjects
{
    public class LoreEntryToTrack
    {
        public string EntryId { get; set; }
        public string LorebookId { get; set; }
        public int Sticky { get; set; }
        public int Cooldown { get; set; }
        public string LinkedMessageId { get; set; }
    }
}
