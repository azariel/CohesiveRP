using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;

namespace CohesiveRP.Core.PromptContext.Abstractions
{
    public interface IPromptContextBuilderFactory
    {
        Task<IPromptContextBuilder> GenerateAsync(BackgroundQuerySystemTags generationTag, IPromptContextElementBuilderFactory promptContextElementBuilderFactory);
    }
}
