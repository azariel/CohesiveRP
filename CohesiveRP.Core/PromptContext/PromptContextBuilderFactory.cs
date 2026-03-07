using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Summary;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.PromptContext
{
    public class PromptContextBuilderFactory : IPromptContextBuilderFactory
    {
        public async Task<IPromptContextBuilder> GenerateAsync(BackgroundQuerySystemTags generationTag, IPromptContextElementBuilderFactory promptContextElementBuilderFactory, IStorageService storageService, string contextLinkedId)
        {
            switch (generationTag)
            {
                case BackgroundQuerySystemTags.main:
                    return new PromptContextBuilder(ChatCompletionPresetType.Main, promptContextElementBuilderFactory, storageService, contextLinkedId);
                case BackgroundQuerySystemTags.sceneTracker:
                    return null;
                case BackgroundQuerySystemTags.shortSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.Summarize, promptContextElementBuilderFactory, storageService, contextLinkedId);
                case BackgroundQuerySystemTags.mediumSummary:
                    return null;
                case BackgroundQuerySystemTags.longSummary:
                    return null;
                case BackgroundQuerySystemTags.extraSummary:
                    return null;
                case BackgroundQuerySystemTags.custom:
                    return null;
                default:
                    return null;
            }
        }
    }
}
