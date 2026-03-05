namespace CohesiveRP.Core.PromptContext.Abstractions
{
    public interface IPromptContextBuilder
    {
        Task<IPromptContext> BuildAsync(string chatId);
    }
}
