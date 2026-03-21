using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.LorebookInstances;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Lorebook instances.
    /// </summary>
    public class LorebookInstancesDal : StorageDal, ILorebookInstanceDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public LorebookInstancesDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<LorebookInstanceDbModel[]> GetLorebookInstancesAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.LorebookInstances.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("c161aa51-e6d6-4fde-9d6a-4cfbb10ca675", $"Error when querying Db on table Lorebook instances to fetch all lorebook instances.", ex);
                return null;
            }
        }

        public async Task<LorebookInstanceDbModel[]> GetLorebookInstancesAsync(Func<LorebookInstanceDbModel, bool> func)
        {
            if (func == null)
            {
                return null;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var result = await dbContext.LorebookInstances.AsAsyncEnumerable().Where(w => func.Invoke(w)).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("104f3e0b-f863-454e-8264-d216adbdf3bc", $"Error when querying (Func) Db on table Lorebook Instances.", ex);
                return null;
            }
        }

        public async Task<LorebookInstanceDbModel> AddLorebookInstanceAsync(LorebookInstanceDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Force those fields
                dbModel.LorebookInstanceId = Guid.NewGuid().ToString();
                dbModel.CreatedAtUtc = DateTime.UtcNow;

                var result = await dbContext.LorebookInstances.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("f49b1242-bb3f-486f-a594-14db10e48a1c", $"Error when querying Db on table Lorebook Instances. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("182fc330-7fb9-479d-8bde-f59d9f30f3c9", $"Error when querying Db on table Lorebook Instances.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateLorebookInstanceAsync(LorebookInstanceDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var lorebookInstance = dbContext.LorebookInstances.FirstOrDefault(w => w.LorebookInstanceId == dbModel.LorebookInstanceId);

                if (lorebookInstance == null)
                {
                    LoggingManager.LogToFile("e9f151f2-ef23-4ed4-8c35-6469ac3f0cb7", $"Lorebook Instance [{dbModel.LorebookInstanceId}] to update wasn't found in storage.");
                    return false;
                }

                // Only handle overridable fields
                lorebookInstance.Entries = dbModel.Entries;

                var result = dbContext.LorebookInstances.Update(lorebookInstance);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("3688d9d2-718c-4815-9272-799a3b2c0e07", $"Error when updating Lorebook Instance. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("0c326c0f-98fe-4d74-be6f-34a7da4de2b2", $"Error when updating an entry on table Lorebook Instances.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteLorebookInstanceAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var lorebookInstance = dbContext.LorebookInstances.FirstOrDefault(w => w.ChatId == chatId);

                if (lorebookInstance == null)
                {
                    LoggingManager.LogToFile("d6b7136e-5eb5-45e2-b1b7-3fa97fb5bc47", $"Lorebook Instance tied to chatId [{chatId}] to update wasn't found in storage.");
                    return false;
                }

                var result = dbContext.LorebookInstances.Remove(lorebookInstance);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("af96d1cd-2a80-4abc-8592-af175de0c6d7", $"Error when deleting a Lorebook Instance. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("53265425-7115-4ec8-bfff-e0f17ee2ed1d", $"Error when deleting entry on table Lorebook Instances.", ex);
                return false;
            }
        }
    }
}
