namespace CohesiveRP.Core.PromptContext.Builders
{
    public interface IPromptContextElementBuilder
    {
        Task<string> BuildAsync();
    }
}
