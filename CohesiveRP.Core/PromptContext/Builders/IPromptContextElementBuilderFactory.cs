using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public interface IPromptContextElementBuilderFactory
    {
        Task<IPromptContextElementBuilder> GenerateBuilderAsync(PromptContextFormatElement contextElement);
    }
}
