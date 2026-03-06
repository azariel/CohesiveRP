using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.LLMProviderManager
{
    public interface ILLMQueryProcessor
    {
        Task ProcessCompletedQueryAsync();
        Task<bool> QueueProcessAsync();
        Task<BackgroundQueryDbModel> GetBackgroundQueryDbModelAsync();
    }
}
