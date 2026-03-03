using CohesiveRP.Core.PromptContext.Format;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public interface IPromptContextElementBuilder
    {
        Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement);
    }
}
