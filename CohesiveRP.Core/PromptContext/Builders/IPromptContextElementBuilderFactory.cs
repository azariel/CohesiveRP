using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Settings;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public interface IPromptContextElementBuilderFactory
    {
        Task<IPromptContextElementBuilder> GenerateBuilderAsync(PromptContextFormatElement contextElement, GlobalSettingsDbModel settings, ChatDbModel chatDbModel, string contextLinkedId, BackgroundQuerySystemTags tag);
    }
}
