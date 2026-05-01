using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Summary;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.PromptContext
{
    public class PromptContextBuilderFactory : IPromptContextBuilderFactory
    {
        public async Task<IPromptContextBuilder> GenerateAsync(BackgroundQuerySystemTags generationTag, IPromptContextElementBuilderFactory promptContextElementBuilderFactory, IStorageService storageService, BackgroundQueryDbModel backgroundQuery)
        {
            GlobalSettingsDbModel globalSettings = await storageService.GetGlobalSettingsAsync();
            switch (generationTag)
            {
                case BackgroundQuerySystemTags.main:
                    return new PromptContextBuilder(ChatCompletionPresetType.Main, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.sceneTracker:
                    return new PromptContextBuilder(ChatCompletionPresetType.SceneTracker, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.sceneAnalyze:
                    return new PromptContextBuilder(ChatCompletionPresetType.SceneAnalyze, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.shortSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.Summarize, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.mediumSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.SummariesMerge, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.longSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.SummariesMerge, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.extraSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.SummariesMerge, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.overflowSummary:
                    return new PromptContextBuilder(ChatCompletionPresetType.SummariesMerge, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.skillChecksInitiator:
                    return new PromptContextBuilder(ChatCompletionPresetType.SkillChecksInitiator, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.dynamicCharacterCreation:
                    return new PromptContextBuilder(ChatCompletionPresetType.DynamicCharacterCreation, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.dynamicCharacterSheetCreation:
                    return new PromptContextBuilder(ChatCompletionPresetType.DynamicCharacterSheetCreation, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.illustrationPromptInjectionForCharacterAvatar:
                    return new PromptContextBuilder(ChatCompletionPresetType.IllustrationPromptInjectionForCharacterAvatar, promptContextElementBuilderFactory, storageService, globalSettings, backgroundQuery, generationTag);
                case BackgroundQuerySystemTags.custom:
                    return null;
                default:
                    return null;
            }
        }
    }
}
