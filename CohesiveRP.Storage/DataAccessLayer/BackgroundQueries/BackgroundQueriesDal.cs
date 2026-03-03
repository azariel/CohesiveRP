using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Global Settings.
    /// </summary>
    public class BackgroundQueriesDal : StorageDal, IBackgroundQueriesDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public BackgroundQueriesDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            Cleanup();
        }

        private void Cleanup()
        {
            using var dbContext = contextFactory.CreateDbContext();

            int NbBackgroundQueries = dbContext.BackgroundQueries.Count();

            if (NbBackgroundQueries <= 0)
            {
                return;
            }

            // Reset status of all BG tasks to Pending if they were InProgress
            var inProgressQueries = dbContext.BackgroundQueries.Where(w => w.Status == BackgroundQueryStatus.InProgress.ToString()).ToArray();
            foreach (var inProgressQuery in inProgressQueries)
            {
                inProgressQuery.Status = BackgroundQueryStatus.Pending.ToString();
                inProgressQuery.Content = string.Empty;
            }

            dbContext.SaveChanges();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************

        public async Task<BackgroundQueryDbModel> CreateBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Add the query to storage
                var dbModel = new BackgroundQueryDbModel()
                {
                    BackgroundQueryId = Guid.NewGuid().ToString(),
                    InsertDateTimeUtc = DateTime.UtcNow,
                    ChatId = queryModel.ChatId,
                    Status = BackgroundQueryStatus.Pending.ToString(),
                    Tags = JsonCommonSerializer.SerializeToString(queryModel.Tags),
                    DependenciesTags = JsonCommonSerializer.SerializeToString(queryModel.DependenciesTags),
                    Priority = (int)queryModel.Priority,
                };

                EntityEntry<BackgroundQueryDbModel> result = await dbContext.BackgroundQueries.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("ef99e0e8-d484-463d-b31f-4948048b54f0", $"Error when querying Db on table BackgroundQueries. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("e510715f-618b-4d14-a9af-fadccbcdf410", $"Error when querying Db on table BackgroundQueries.", ex);
                return null;
            }
        }

        public async Task<BackgroundQueryDbModel[]> GetAllPendingQueriesAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.BackgroundQueries.Where(w => w.Status == BackgroundQueryStatus.Pending.ToString()).ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("13272c00-4ba1-4815-a461-3b6abc9516b3", $"Error when querying pending queries on table BackgroundQueries.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateBackgroundQueryAsync(BackgroundQueryDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                EntityEntry<BackgroundQueryDbModel> result = dbContext.BackgroundQueries.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("e437e752-7349-46cc-801f-d912267ec2a8", $"Error when updating Dbmodel on table BackgroundQueries. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("cae4a655-b44d-4d6d-ae3a-5424fc0206aa", $"Error when querying pending queries on table BackgroundQueries.", ex);
                return false;
            }
        }

        public async Task<BackgroundQueryDbModel> GetBackgroundQueryAsync(string queryId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.BackgroundQueries.FirstOrDefault(f=>f.BackgroundQueryId == queryId);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("16533a30-0c2c-429c-9287-17db226fd947", $"Error when querying query by id [{queryId}] on table BackgroundQueries.", ex);
                return null;
            }
        }
    }
}
