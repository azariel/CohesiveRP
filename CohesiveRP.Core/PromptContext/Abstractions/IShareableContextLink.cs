using CohesiveRP.Core.PromptContext.Builders;

namespace CohesiveRP.Core.PromptContext.Abstractions
{
    public interface IShareableContextLink
    {
        IPromptContextElementBuilder LinkedBuilder { get; init; }
        object Value { get; init; }
    }
}
