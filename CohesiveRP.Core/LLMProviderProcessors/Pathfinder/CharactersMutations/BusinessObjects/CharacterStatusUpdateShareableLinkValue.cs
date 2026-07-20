namespace CohesiveRP.Core.LLMProviderProcessors.Pathfinder.CharactersMutations.BusinessObjects
{
    public class CharacterStatusUpdateShareableLinkValue
    {
        public List<string> CharacterSheetInstanceIds { get; set; }
        public string LastIncludedMessageId { get; set; }
    }
}