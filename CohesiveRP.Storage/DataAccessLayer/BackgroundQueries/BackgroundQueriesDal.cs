using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Global Settings.
    /// </summary>
    public class BackgroundQueriesDal : StorageDal, IBackgroundQueriesDal, IDisposable
    {
        private StorageDbContext storageDbContext;

        public BackgroundQueriesDal(JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
        {
            storageDbContext = new StorageDbContext();
            storageDbContext.BackgroundQueries.Load();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public void Dispose()
        {
            storageDbContext?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<BackgroundQueryDbModel> CreateBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel)
        {
            try
            {
                await storageDbContext.BackgroundQueries.LoadAsync();

                // Add the query to storage
                var dbModel = new BackgroundQueryDbModel()
                {
                    BackgroundQueryId = Guid.NewGuid().ToString(),
                    InsertDateTimeUtc = DateTime.UtcNow,
                    Tags = JsonCommonSerializer.SerializeToString(queryModel.Tags),
                };

                EntityEntry<BackgroundQueryDbModel> result = await storageDbContext.BackgroundQueries.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("ef99e0e8-d484-463d-b31f-4948048b54f0", $"Error when querying Db on table BackgroundQueries. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await storageDbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("e510715f-618b-4d14-a9af-fadccbcdf410", $"Error when querying Db on table BackgroundQueries.", ex);
                return null;
            }
        }
    }
}
