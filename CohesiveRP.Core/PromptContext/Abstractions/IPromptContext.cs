namespace CohesiveRP.Core.PromptContext.Abstractions
{
    public interface IPromptContext
    {
        string Value { get; set; }
        IPromptMessage[] Messages { get; set; }

        // Object to share information between the builder and the completion processor function. For ex: The builder could query 3 specific messages and pass those Ids to the completion so that we can tag them.
        List<IShareableContextLink> ShareableContextLinks { get; set; }
    }
}
