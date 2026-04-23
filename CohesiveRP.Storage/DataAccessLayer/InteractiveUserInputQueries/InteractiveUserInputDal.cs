using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries
{
    /// <summary>
    /// DataAccessLayer around interactive user inputs.
    /// </summary>
    public class InteractiveUserInputDal : StorageDal, IInteractiveUserInputDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public InteractiveUserInputDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;
            Cleanup();
        }

        private void Cleanup()
        {
            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();

            // Always clear completed rows in this table upon startup
            dbContext.InteractiveUserInputQueries.Where(w => w.Status == BusinessObjects.InteractiveUserInputStatus.Completed).ExecuteDelete();// CharacterSheet was created, which mean that the process won't try to generate a new card for this specific character, we can safely delete the InteractiveUserInputQueries that are completed.

            // Reset InProgress queries
            var inProgressQueries = dbContext.InteractiveUserInputQueries.Where(w => w.Status == BusinessObjects.InteractiveUserInputStatus.Processing).ToList();
            foreach (var item in inProgressQueries)
            {
                item.Status = BusinessObjects.InteractiveUserInputStatus.Pending;
            }

            dbContext.SaveChanges();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<InteractiveUserInputDbModel[]> GetInteractiveUserInputQueriesAsync(Func<InteractiveUserInputDbModel, bool> func)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if(func == null)
                    return dbContext.InteractiveUserInputQueries.ToArray();

                var result = await dbContext.InteractiveUserInputQueries.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("99054478-3488-49df-898d-e4776e33b88d", $"Error when querying Db on table InteractiveUserInputQueries.", ex);
                return null;
            }
        }

        public async Task<InteractiveUserInputDbModel[]> GetInteractiveUserInputQueriesAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return await dbContext.InteractiveUserInputQueries.ToArrayAsync();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("a7025c32-85fa-4e6a-bbb0-4082117595fe", $"Error when querying Db on table InteractiveUserInputQueries.", ex);
                return null;
            }
        }

        public async Task<InteractiveUserInputDbModel> AddInteractiveUserInputQueryAsync(InteractiveUserInputDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Force those fields
                dbModel.InteractiveUserInputQueryId = Guid.NewGuid().ToString();
                dbModel.CreatedAtUtc = DateTime.UtcNow;

                var result = await dbContext.InteractiveUserInputQueries.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("c9dc3866-4bfd-469d-82f1-6e0caabb01a9", $"Error when querying Db on table InteractiveUserInputQueries. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("a3f2dcd9-30c2-4dd5-9af2-86d848ef76ac", $"Error when querying Db on table InteractiveUserInputQueries.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateInteractiveUserInputQueryAsync(InteractiveUserInputDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.InteractiveUserInputQueries.FirstOrDefault(w => w.InteractiveUserInputQueryId == dbModel.InteractiveUserInputQueryId);

                if (item == null)
                {
                    LoggingManager.LogToFile("f1904cb6-8f86-482e-99be-626b56d51efd", $"InteractiveUserInputQuery [{dbModel.InteractiveUserInputQueryId}] to update wasn't found in storage.");
                    return false;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.SceneTrackerId = item.SceneTrackerId;
                dbModel.ChatId = item.ChatId;
                dbModel.Type = item.Type;

                var result = dbContext.InteractiveUserInputQueries.Update(item);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("87d21c78-228c-4870-8637-dc49b4fbd7cd", $"Error when updating InteractiveUserInputQuery. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("dc8df8dc-b99b-4651-81c8-8afd79fdc2df", $"Error when querying pending queries on table InteractiveUserInputQueries.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteInteractiveUserInputQueryAsync(string interactionUserInputQueryId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.InteractiveUserInputQueries.AsNoTracking().FirstOrDefault(w => w.InteractiveUserInputQueryId == interactionUserInputQueryId);

                if (item == null)
                {
                    LoggingManager.LogToFile("7f5b8965-8104-47b7-ada5-e7b2349aba76", $"InteractiveUserInputQuery [{interactionUserInputQueryId}] to delete wasn't found in storage.");
                    return false;
                }

                var result = dbContext.InteractiveUserInputQueries.Remove(item);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("9f0cf225-3d2e-4b80-9d0b-7342a60b7fda", $"Error when deleting a specific InteractiveUserInputQuery [{interactionUserInputQueryId}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("d122c128-8385-43e9-a7c8-e40bee40be2b", $"Error when querying pending queries on table InteractiveUserInputQueries.", ex);
                return false;
            }
        }
    }
}
