using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;

namespace CohesiveRP.Core.PromptContext
{
    public class ShareableContextLink : IShareableContextLink
    {
        public IPromptContextElementBuilder LinkedBuilder { get; init; }

        public object Value { get; init; }
    }
}
