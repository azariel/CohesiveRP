using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.SceneTracker
{
    public interface ISceneTrackerPostProcess
    {
        Task<bool> Process(ChatCompletionPresetType completionPresetType, BackgroundQuerySystemTags? tag, BackgroundQueryDbModel backgroundQueryDbModel, PromptContext.Abstractions.IShareableContextLink shareableContextLink, string AImessage);
    }
}
