using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Main;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;

namespace CohesiveRP.Core.PromptContext
{
    public class PromptContextBuilderFactory : IPromptContextBuilderFactory
    {
        public async Task<IPromptContextBuilder> GenerateAsync(BackgroundQuerySystemTags generationTag, IPromptContextElementBuilderFactory promptContextElementBuilderFactory, IStorageService storageService)
        {
            switch (generationTag)
            {
                case BackgroundQuerySystemTags.main:
                    return new MainPromptContextBuilder(promptContextElementBuilderFactory, storageService);
                case BackgroundQuerySystemTags.sceneTracker:
                    return null;
                case BackgroundQuerySystemTags.summary:
                    return null;
                case BackgroundQuerySystemTags.custom:
                    return null;
                default:
                    return null;
            }
        }
    }
}
