namespace CohesiveRP.Storage.DataAccessLayer.Settings
{
    public interface IGlobalSettingsDal
    {
        Task<GlobalSettingsDbModel> GetGlobalSettingsAsync();
        Task<bool> UpdateGlobalSettingsAsync(GlobalSettingsDbModel dbModel);
    }
}
