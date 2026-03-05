using CohesiveRP.Core.LLMProviderManager.Main;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;

namespace CohesiveRP.Core.LLMProviderManager
{
    public class LLMProviderQueryerFactory : ILLMProviderQueryerFactory
    {
        private IPromptContextBuilderFactory promptContextBuilderFactory;
        private IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
        private IStorageService storageService;

        public LLMProviderQueryerFactory(IPromptContextBuilderFactory promptContextBuilderFactory, IPromptContextElementBuilderFactory promptContextElementBuilderFactory, IStorageService storageService)
        {
           this.promptContextBuilderFactory = promptContextBuilderFactory;
           this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
           this.storageService = storageService;
        }

        private BackgroundQuerySystemTags GetRunningTagFromTags(List<string> tags)
        {
            if(tags.Contains(BackgroundQuerySystemTags.main.ToString()))
                return BackgroundQuerySystemTags.main;

            if(tags.Contains(BackgroundQuerySystemTags.sceneTracker.ToString()))
                return BackgroundQuerySystemTags.sceneTracker;

            if(tags.Contains(BackgroundQuerySystemTags.summary.ToString()))
                return BackgroundQuerySystemTags.summary;

            return BackgroundQuerySystemTags.custom;
        }

        public ILLMQueryProcessor Generate(BackgroundQueryDbModel queryModel)
        {
            if(queryModel == null)
            {
                return null;
            }

            var runningTag = GetRunningTagFromTags(queryModel.Tags);

            switch (runningTag)
            {
                case BackgroundQuerySystemTags.main:
                    return new MainLLMQueryProcessor(queryModel, promptContextBuilderFactory, promptContextElementBuilderFactory, storageService);
                case BackgroundQuerySystemTags.sceneTracker:
                    return null;
                case BackgroundQuerySystemTags.summary:
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
