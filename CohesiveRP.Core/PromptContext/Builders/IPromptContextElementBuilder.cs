using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public interface IPromptContextElementBuilder
    {
        Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement);
    }
}
