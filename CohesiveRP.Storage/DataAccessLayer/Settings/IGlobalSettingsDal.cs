using CohesiveRP.Storage.Users;

namespace CohesiveRP.Storage.DataAccessLayer.Settings
{
    public interface IGlobalSettingsDal
    {
        Task<GlobalSettingsDbModel> GetGlobalSettingsAsync();
    }
}
