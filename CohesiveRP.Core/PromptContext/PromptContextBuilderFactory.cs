using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Main;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;

namespace CohesiveRP.Core.PromptContext
{
    public class PromptContextBuilderFactory : IPromptContextBuilderFactory
    {
        public async Task<IPromptContextBuilder> GenerateAsync(BackgroundQuerySystemTags generationTag, IPromptContextElementBuilderFactory promptContextElementBuilderFactory)
        {
            switch (generationTag)
            {
                case BackgroundQuerySystemTags.main:
                    return new MainPromptContextBuilder(promptContextElementBuilderFactory);
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
