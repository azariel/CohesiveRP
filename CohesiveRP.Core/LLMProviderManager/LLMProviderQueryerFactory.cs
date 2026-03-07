using CohesiveRP.Core.LLMProviderManager.Main;
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
                    return null;
                case BackgroundQuerySystemTags.shortSummary:
                    return new ShortSummaryLLMQueryProcessor(ChatCompletionPresetType.Summarize, BackgroundQuerySystemTags.shortSummary, queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService, httpLLMApiProviderService, summaryService);
                case BackgroundQuerySystemTags.mediumSummary:
                    return null;
                case BackgroundQuerySystemTags.longSummary:
                    return null;
                case BackgroundQuerySystemTags.extraSummary:
                    return null;
                case BackgroundQuerySystemTags.custom:
                    break;
                default:
                    return null;
            }

            return null;
        }
    }
}
