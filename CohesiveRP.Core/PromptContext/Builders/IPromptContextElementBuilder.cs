using CohesiveRP.Core.PromptContext.Abstractions;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public interface IPromptContextElementBuilder
    {
        Task<(string result, IShareableContextLink link)> BuildAsync();
    }
}
