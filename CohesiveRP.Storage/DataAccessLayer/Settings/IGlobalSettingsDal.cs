using CohesiveRP.Storage.Users;

namespace CohesiveRP.Storage.DataAccessLayer.AIQueries
{
    public interface IGlobalSettingsDal
    {
        Task<GlobalSettingsDbModel> GetGlobalSettingsAsync();
    }
}
