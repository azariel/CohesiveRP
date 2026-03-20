using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using static CohesiveRP.Common.Diagnostics.LoggingManager;

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
            dbContext.Database.EnsureCreated();

            int NbBackgroundQueries = dbContext.BackgroundQueries.Count();

            RemoveCompletedOrErrorsQueries(dbContext);

            if (NbBackgroundQueries <= 0)
            {
                return;
            }

            // Reset status of all BG tasks to Pending if they were InProgress
            var inProgressQueries = dbContext.BackgroundQueries.Where(w => w.Status == BackgroundQueryStatus.InProgress).ToArray();
            foreach (var inProgressQuery in inProgressQueries)
            {
                inProgressQuery.Status = BackgroundQueryStatus.Pending;
                inProgressQuery.Content = string.Empty;
            }

            var processingQueries = dbContext.BackgroundQueries.Where(w => w.Status == BackgroundQueryStatus.ProcessingFinalInstruction).ToArray();
            foreach (var processingQuery in processingQueries)
            {
                // Note: Keep content as-is since it was correctly generated, but reset the query to a status to process that content before setting it to completed
                processingQuery.Status = BackgroundQueryStatus.ProcessedWaitingForFinalInstruction;
            }

            dbContext.SaveChanges();
        }

        private void RemoveCompletedOrErrorsQueries(StorageDbContext dbContext)
        {
            var bgQueriesToDelete = dbContext.BackgroundQueries.Where(w => w.Status == BackgroundQueryStatus.Completed || w.Status == BackgroundQueryStatus.Error).ToArray();
            _ = DeleteBackgroundQueriesAsync(bgQueriesToDelete);
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
                    LinkedId = queryModel.LinkedId,
                    CreatedAtUtc = DateTime.UtcNow,
                    ChatId = queryModel.ChatId,
                    Status = BackgroundQueryStatus.Pending,
                    Tags = queryModel.Tags,
                    DependenciesTags = queryModel.DependenciesTags,
                    Priority = queryModel.Priority,
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
                return dbContext.BackgroundQueries.Where(w => w.Status == BackgroundQueryStatus.Pending || w.Status == BackgroundQueryStatus.ProcessedWaitingForFinalInstruction).ToArray();
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

        public async Task<BackgroundQueryDbModel> GetBackgroundQueryByFuncAsync(Func<DbSet<BackgroundQueryDbModel>, BackgroundQueryDbModel> func)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return func.Invoke(dbContext.BackgroundQueries);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ebdb24b4-0b55-4582-9052-70085c4a64d1", $"Error when querying query by func [{func}] on table BackgroundQueries.", ex);
                return null;
            }
        }

        public async Task<BackgroundQueryDbModel[]> GetBackgroundQueriesByFuncAsync(Func<DbSet<BackgroundQueryDbModel>, BackgroundQueryDbModel[]> func)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return func.Invoke(dbContext.BackgroundQueries);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("8ae29263-3b84-4d06-a316-3449e095a39b", $"Error when querying query by func [{func}] on table BackgroundQueries.", ex);
                return null;
            }
        }

        public async Task<BackgroundQueryDbModel> GetBackgroundQueryAsync(string queryId)
        {
            return await GetBackgroundQueryByFuncAsync(f => f.FirstOrDefault(f => f.BackgroundQueryId == queryId));
        }

        public async Task<BackgroundQueryDbModel[]> GetBackgroundQueriesByChatIdAsync(string chatId)
        {
            return await GetBackgroundQueriesByFuncAsync(f => f.Where(f => f.ChatId == chatId && (f.Status != BackgroundQueryStatus.Completed && f.Status != BackgroundQueryStatus.Error)).ToArray());
        }

        public async Task<BackgroundQueryDbModel[]> GetPendingOrProcessingBackgroundQueryAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.BackgroundQueries.Where(w =>
                    w.Status == BackgroundQueryStatus.Pending ||
                    w.Status == BackgroundQueryStatus.InProgress ||
                    w.Status == BackgroundQueryStatus.ProcessingFinalInstruction ||
                    w.Status == BackgroundQueryStatus.ProcessedWaitingForFinalInstruction
                ).ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("6a5dfca3-31a0-4dd4-b8f4-8126524d6c48", $"Error when querying pending or unprocessed queries on table BackgroundQueries.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteBackgroundQueryAsync(BackgroundQueryDbModel dbModel)
        {
            return await DeleteBackgroundQueriesAsync([dbModel]);
        }

        public async Task<bool> DeleteBackgroundQueriesAsync(BackgroundQueryDbModel[] dbModels)
        {
            try
            {
                var idsToDelete = dbModels.Select(m => m.BackgroundQueryId).ToArray();
                using var dbContext = await contextFactory.CreateDbContextAsync();
                BackgroundQueryDbModel[] queries = dbContext.BackgroundQueries.Where(w => idsToDelete.Contains(w.BackgroundQueryId)).ToArray();

                if (queries == null || queries.Length <= 0)
                {
                    LoggingManager.LogToFile("7d9a336d-6810-4808-b0e5-d5e73989c63b", $"BackgroundQueries [{string.Join(",", dbModels.Select(s => s.BackgroundQueryId))}] to update weren't found in storage.");
                    return false;
                }

                dbContext.BackgroundQueries.RemoveRange(queries);
                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("d4f5ffc3-a05d-4843-9360-c9bc4b34ed57", $"Error when querying pending queries on table BackgroundQueries.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteBackgroundQueriesByChatIdAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var backgroundQueryDbModels = dbContext.BackgroundQueries.Where(w => w.ChatId == chatId).ToArray();

                if (backgroundQueryDbModels == null)
                {
                    LoggingManager.LogToFile("6f58eb8e-7c89-4e8f-9c2b-20430dc30085", $"Chat [{chatId}] did not have any tethered backgroundQueries to delete in storage.", logVerbosity: LogVerbosity.Verbose);
                    return false;
                }

                dbContext.BackgroundQueries.RemoveRange(backgroundQueryDbModels);
                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("8173a545-f6d2-43c8-8caf-6b1bcf5e497c", $"Error when querying pending queries on table Chats.", ex);
                return false;
            }
        }
    }
}
