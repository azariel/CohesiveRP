using CohesiveRP.Storage.QueryModels.BackgroundQuery;

namespace CohesiveRP.Storage.DataAccessLayer.AIQueries
{
    public interface IBackgroundQueriesDal
    {
        Task<BackgroundQueryDbModel> CreateBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel);
        Task<BackgroundQueryDbModel[]> GetAllPendingQueriesAsync();
        Task<bool> UpdateBackgroundQueryAsync(BackgroundQueryDbModel selectedQuery);
        Task<BackgroundQueryDbModel> GetBackgroundQueryAsync(string queryId);
    }
}
