using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public interface IPromptContextElementBuilderFactory
    {
        Task<IPromptContextElementBuilder> GenerateBuilderAsync(PromptContextFormatElement contextElement, ChatDbModel chatDbModel);
    }
}
