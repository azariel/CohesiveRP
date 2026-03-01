using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.Users;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Global Settings.
    /// </summary>
    public class GlobalSettingsDal : StorageDal, IGlobalSettingsDal, IDisposable
    {
        private StorageDbContext storageDbContext;

        public GlobalSettingsDal(JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
        {
            storageDbContext = new StorageDbContext();
            storageDbContext.GlobalSettings.Load();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public void Dispose()
        {
            storageDbContext?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<GlobalSettingsDbModel> GetGlobalSettingsAsync()
        {
            try
            {
                await storageDbContext.GlobalSettings.LoadAsync();
                return storageDbContext.GlobalSettings.FirstOrDefault();// TODO get by UserId eventually
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("218243a4-2b6f-460e-a99c-5375e5c97ad9", $"Error when querying Db on table GlobalSettings.", ex);
                return null;
            }
        }
    }
}
