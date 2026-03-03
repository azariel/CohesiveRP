using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Global Settings.
    /// </summary>
    public class GlobalSettingsDal : StorageDal, IGlobalSettingsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public GlobalSettingsDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            Cleanup();
        }

        private void Cleanup()
        {
            using var dbContext = contextFactory.CreateDbContext();
            int GlobalSettings = dbContext.GlobalSettings.Count();

            if (GlobalSettings <= 0)
            {
                // Create default settings
                dbContext.GlobalSettings.Add(new GlobalSettingsDbModel
                {
                    GlobalSettingsId = Guid.NewGuid().ToString(),
                    InsertDateTimeUtc = DateTime.UtcNow,
                    // TODO: replace this dev option
                    LLMProviders = "[{\"providerConfigId\":\"ba92f3f0-923f-4a32-a52b-4767ee7f4a1c\",\"name\":\"IntenseRP-V2-GLM\",\"type\":\"OpenAICustom\",\"concurrencyLimit\":1,\"tags\":[\"main\"],\"timeoutStrategy\":{\"type\":\"RetryXtimesThenGiveUp\",\"retries\":3}},{\"providerConfigId\":\"191302fb-08bf-4f30-a192-d6f20c42a378\",\"name\":\"IntenseRP-V2-DS\",\"type\":\"OpenAICustom\",\"concurrencyLimit\":1,\"tags\":[\"sceneTracker\",\"summary\"],\"timeoutStrategy\":{\"type\":\"RetryXtimesThenGiveUp\",\"retries\":3}}]"
                });

                dbContext.SaveChangesAsync();
                return;
            }
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<GlobalSettingsDbModel> GetGlobalSettingsAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                //await dbContext.GlobalSettings.LoadAsync();
                return dbContext.GlobalSettings.FirstOrDefault();// TODO get by UserId eventually
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("218243a4-2b6f-460e-a99c-5375e5c97ad9", $"Error when querying Db on table GlobalSettings.", ex);
                return null;
            }
        }
    }
}
