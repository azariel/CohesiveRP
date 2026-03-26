using CohesiveRP.Core.LLMProviderManager.Main;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator;
using CohesiveRP.Core.LLMProviderProcessors.SceneTracker;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderManager
{
    public class LLMProviderQueryerFactory : ILLMProviderQueryerFactory
    {
        private IPromptContextBuilderFactory promptContextBuilderFactory;
        private IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
        private IStorageService storageService;
        private IHttpLLMApiProviderService httpLLMApiProviderService;
        private ISummaryService summaryService;

        public LLMProviderQueryerFactory(
            IPromptContextBuilderFactory promptContextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService,
            ISummaryService summaryService)
        {
            this.promptContextBuilderFactory = promptContextBuilderFactory;
            this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
            this.storageService = storageService;
            this.httpLLMApiProviderService = httpLLMApiProviderService;
            this.summaryService = summaryService;
        }

        private BackgroundQuerySystemTags GetRunningTagFromTags(List<string> tags)
        {
            if (tags.Contains(BackgroundQuerySystemTags.main.ToString()))
                return BackgroundQuerySystemTags.main;

            if (tags.Contains(BackgroundQuerySystemTags.sceneTracker.ToString()))
                return BackgroundQuerySystemTags.sceneTracker;

            if (tags.Contains(BackgroundQuerySystemTags.shortSummary.ToString()))
                return BackgroundQuerySystemTags.shortSummary;

            if (tags.Contains(BackgroundQuerySystemTags.mediumSummary.ToString()))
                return BackgroundQuerySystemTags.mediumSummary;

            if (tags.Contains(BackgroundQuerySystemTags.longSummary.ToString()))
                return BackgroundQuerySystemTags.longSummary;

            if (tags.Contains(BackgroundQuerySystemTags.extraSummary.ToString()))
                return BackgroundQuerySystemTags.extraSummary;

            if (tags.Contains(BackgroundQuerySystemTags.overflowSummary.ToString()))
                return BackgroundQuerySystemTags.overflowSummary;

            if (tags.Contains(BackgroundQuerySystemTags.skillChecksInitiator.ToString()))
                return BackgroundQuerySystemTags.skillChecksInitiator;

            return BackgroundQuerySystemTags.custom;
        }

        public ILLMQueryProcessor Generate(BackgroundQueryDbModel queryModel)
        {
            if (queryModel == null)
            {
                return null;
            }

            var runningTag = GetRunningTagFromTags(queryModel.Tags);

            switch (runningTag)
            {
                case BackgroundQuerySystemTags.main:
                    return new MainLLMQueryProcessor(ChatCompletionPresetType.Main, BackgroundQuerySystemTags.main, queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
                case BackgroundQuerySystemTags.sceneTracker:
                    return new SceneTrackerLLMQueryProcessor(ChatCompletionPresetType.SceneTracker, BackgroundQuerySystemTags.sceneTracker, queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
                case BackgroundQuerySystemTags.shortSummary:
                    return new ShortSummaryLLMQueryProcessor(ChatCompletionPresetType.Summarize, BackgroundQuerySystemTags.shortSummary, queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
                case BackgroundQuerySystemTags.mediumSummary:
                    return new SummaryMergerLLMQueryProcessor(ChatCompletionPresetType.SummariesMerge, BackgroundQuerySystemTags.mediumSummary, queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
                case BackgroundQuerySystemTags.longSummary:
                    return new SummaryMergerLLMQueryProcessor(ChatCompletionPresetType.SummariesMerge, BackgroundQuerySystemTags.longSummary, queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
                case BackgroundQuerySystemTags.extraSummary:
                    return new SummaryMergerLLMQueryProcessor(ChatCompletionPresetType.SummariesMerge, BackgroundQuerySystemTags.extraSummary, queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
                case BackgroundQuerySystemTags.overflowSummary:
                    return new SummaryMergerLLMQueryProcessor(ChatCompletionPresetType.SummariesMerge, BackgroundQuerySystemTags.overflowSummary, queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
                case BackgroundQuerySystemTags.skillChecksInitiator:
                    return new SkillChecksInitiatorLLMQueryProcessor(ChatCompletionPresetType.SkillChecksInitiator, BackgroundQuerySystemTags.skillChecksInitiator, queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
                case BackgroundQuerySystemTags.custom:
                    break;
                default:
                    return null;
            }

            return null;
        }

        //public ILLMQueryProcessor GenerateRegenerateCharacterSheetProcessor()
        //{
        //     return new RegenerateCharacterSheetQueryProcessor(ChatCompletionPresetType.RegenerateCharacterSheet, null, null, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
        //}
    }
}
