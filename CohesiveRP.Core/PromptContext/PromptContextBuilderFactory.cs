using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Summary;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.PromptContext
{
    public class PromptContextBuilderFactory : IPromptContextBuilderFactory
    {
        public async Task<IPromptContextBuilder> GenerateAsync(BackgroundQuerySystemTags generationTag, IPromptContextElementBuilderFactory promptContextElementBuilderFactory, IStorageService storageService, string contextLinkedId)
        {
            GlobalSettingsDbModel globalSettings = await storageService.GetGlobalSettingsAsync();
            switch (generationTag)
            {
                case BackgroundQuerySystemTags.main:
                    return new PromptContextBuilder(ChatCompletionPresetType.Main, promptContextElementBuilderFactory, storageService, globalSettings, contextLinkedId, generationTag);
                case BackgroundQuerySystemTags.sceneTracker:
                    return null;
                case BackgroundQuerySystemTags.shortSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.Summarize, promptContextElementBuilderFactory, storageService, globalSettings, contextLinkedId, generationTag);
                case BackgroundQuerySystemTags.mediumSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.SummariesMerge, promptContextElementBuilderFactory, storageService, globalSettings, contextLinkedId, generationTag);
                case BackgroundQuerySystemTags.longSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.SummariesMerge, promptContextElementBuilderFactory, storageService, globalSettings, contextLinkedId, generationTag);
                case BackgroundQuerySystemTags.extraSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.SummariesMerge, promptContextElementBuilderFactory, storageService, globalSettings, contextLinkedId, generationTag);
                case BackgroundQuerySystemTags.overflowSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.SummariesMerge, promptContextElementBuilderFactory, storageService, globalSettings, contextLinkedId, generationTag);
                case BackgroundQuerySystemTags.custom:
                    return null;
                default:
                    return null;
            }
        }
    }
}
