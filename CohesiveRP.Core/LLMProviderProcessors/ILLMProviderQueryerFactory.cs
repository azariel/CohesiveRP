using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.LLMProviderManager
{
    public interface ILLMProviderQueryerFactory
    {
        Task<ILLMQueryProcessor> GenerateAsync(BackgroundQueryDbModel queryModel);
    }
}
