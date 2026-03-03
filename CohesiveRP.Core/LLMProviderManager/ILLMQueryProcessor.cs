using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.LLMProviderManager
{
    public interface ILLMQueryProcessor
    {
        Task ProcessCompletedQueryAsync(BackgroundQueryDbModel selectedQuery);
        Task<bool> QueueProcessAsync();
        Task<BackgroundQueryDbModel> GetBackgroundQueryDbModelAsync();
    }
}
