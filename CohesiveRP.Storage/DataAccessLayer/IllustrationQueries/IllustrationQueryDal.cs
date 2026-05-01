using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.IllustrationQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.IllustrationQueries
{
    /// <summary>
    /// DataAccessLayer around illustration queries.
    /// </summary>
    public class IllustrationQueryDal : StorageDal, IIllustrationQueryDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public IllustrationQueryDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;
            Cleanup();
        }

        private void Cleanup()
        {
            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();

            // Always clear rows in error or completed in this table upon startup
            dbContext.IllustrationQueries.Where(w => w.Status == IllustratorQueryStatus.Completed || w.Status == IllustratorQueryStatus.Error).ExecuteDelete();

            // Reset InProgress queries
            var inProgressQueries = dbContext.IllustrationQueries.Where(w => w.Status == IllustratorQueryStatus.Processing).ToList();
            foreach (var item in inProgressQueries)
            {
                item.Status = IllustratorQueryStatus.Pending;
            }

            dbContext.SaveChanges();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<IllustrationQueryDbModel[]> GetIllustrationQueriesAsync(Func<IllustrationQueryDbModel, bool> func)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if (func == null)
                    return dbContext.IllustrationQueries.ToArray();

                var result = await dbContext.IllustrationQueries.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("9e81ec22-954f-4629-acb6-60e64c7316e9", $"Error when querying Db on table IllustrationQueries.", ex);
                return null;
            }
        }

        public async Task<IllustrationQueryDbModel> AddIllustrationQueryAsync(IllustrationQueryDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Force system fields
                dbModel.IllustrationQueryId = Guid.NewGuid().ToString();
                dbModel.CreatedAtUtc = DateTime.UtcNow;

                var result = await dbContext.IllustrationQueries.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("7b6f508b-a111-40fc-97c5-f172199dd3e5", $"Error when querying Db on table IllustrationQueries to Add new row. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("c8cdcc79-9176-4dbd-8d35-0fd13dd124de", $"Error when querying Db on table IllustrationQueries to add new row.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateIllustrationQueryAsync(IllustrationQueryDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.IllustrationQueries.AsNoTracking().FirstOrDefault(w => w.IllustrationQueryId == dbModel.IllustrationQueryId);

                if (item == null)
                {
                    LoggingManager.LogToFile("7091321e-01e2-4a3d-8e7e-ee2629e7e9ea", $"IllustrationQuery [{dbModel.IllustrationQueryId}] to update wasn't found in storage.");
                    return false;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.Type = item.Type;
                dbModel.ChatId = item.ChatId;

                var result = dbContext.IllustrationQueries.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("669bb48f-59d4-4f1d-a45b-7b3847df2df6", $"Error when updating IllustrationQuery. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("c1f4b7d4-7f0d-41e8-83c2-fc7cf65584cf", $"Error when querying pending queries on table IllustrationQueries.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteIllustrationQueryAsync(string interactionUserInputQueryId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.IllustrationQueries.AsNoTracking().FirstOrDefault(w => w.IllustrationQueryId == interactionUserInputQueryId);

                if (item == null)
                {
                    LoggingManager.LogToFile("b3d4c658-4be7-4d75-8928-0e155d5dbf12", $"IllustrationQuery [{interactionUserInputQueryId}] to delete wasn't found in storage.");
                    return false;
                }

                var result = dbContext.IllustrationQueries.Remove(item);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("91f550c0-6117-4bc4-8424-9a407182b809", $"Error when deleting a specific IllustrationQuery [{interactionUserInputQueryId}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("3f76c341-b204-4060-aeda-4d6fb7249723", $"Error when querying pending queries on table IllustrationQueries.", ex);
                return false;
            }
        }
    }
}
