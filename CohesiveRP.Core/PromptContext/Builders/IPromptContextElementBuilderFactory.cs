using CohesiveRP.Core.PromptContext.Format;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public interface IPromptContextElementBuilderFactory
    {
        Task<IPromptContextElementBuilder> GenerateBuilderAsync(PromptContextFormatElement contextElement);
    }
}
