using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public interface IPromptContextElementBuilderFactory
    {
        Task<IPromptContextElementBuilder> GenerateBuilderAsync(PromptContextFormatElement contextElement, PromptContextSettings settings, ChatDbModel chatDbModel, string contextLinkedId);
    }
}
