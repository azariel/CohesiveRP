namespace CohesiveRP.Storage.DataAccessLayer.AIQueries
{
    public interface ILLMApiQueriesDal
    {
        Task<LLMApiQueryDbModel> AddNewQueryAsync(LLMApiQueryDbModel newQuery);
        Task<bool> DeleteQueryByIdAsync(string lLMApiQueryId);
        Task<LLMApiQueryDbModel[]> GetQueriesOnLLMApisAsync(string tag);
    }
}
