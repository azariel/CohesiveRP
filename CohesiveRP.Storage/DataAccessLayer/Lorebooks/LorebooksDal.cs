using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.Lorebooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Lorebooks.
    /// </summary>
    public class LorebooksDal : StorageDal, ILorebooksDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public LorebooksDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<LorebookDbModel[]> GetLorebooksAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.Lorebooks.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("aaa064e3-14c4-4c76-9bd7-e7f7373c60ab", $"Error when querying Db on table Lorebooks.", ex);
                return null;
            }
        }

        public async Task<LorebookDbModel[]> GetLorebooksByFuncAsync(Func<LorebookDbModel, bool> func)
        {
            if (func == null)
            {
                return null;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var result = await dbContext.Lorebooks.AsAsyncEnumerable().Where(w => func.Invoke(w)).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("71346817-09ac-462f-b4b7-a8bc39acf79e", $"Error when querying Db on table Lorebooks.", ex);
                return null;
            }
        }

        public async Task<LorebookDbModel> GetLorebookByIdAsync(string id)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.Lorebooks.FirstOrDefault(w => w.LorebookId == id);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("db711628-ba99-4859-912a-cedf7e9a129b", $"Error when querying Db on table Lorebooks.", ex);
                return null;
            }
        }

        public async Task<LorebookDbModel> AddLorebookAsync(AddLorebookQueryModel queryModel)
        {
            try
            {
                // Convert models
                LorebookDbModel LorebookDbModel = new LorebookDbModel
                {
                    Name = queryModel.Name,
                    Entries = queryModel.Entries,
                };

                return await AddLorebookAsync(LorebookDbModel);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("33445017-ae30-43a9-befa-0f90853b7e70", $"Error when querying Db on table Lorebooks.", ex);
                return null;
            }
        }

        public async Task<LorebookDbModel> AddLorebookAsync(LorebookDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Force those fields
                dbModel.LorebookId = Guid.NewGuid().ToString();
                dbModel.CreatedAtUtc = DateTime.UtcNow;
                dbModel.LastActivityAtUtc = DateTime.UtcNow;

                EntityEntry<LorebookDbModel> result = await dbContext.Lorebooks.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("9b826409-2d26-4c91-8c12-996dfef6d355", $"Error when querying Db on table Lorebooks. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("0be9d5e8-3bb4-4040-8fbd-c0e5ded5d40b", $"Error when querying Db on table Lorebooks.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateLorebookAsync(LorebookDbModel lorebookDbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var lorebook = dbContext.Lorebooks.FirstOrDefault(w => w.LorebookId == lorebookDbModel.LorebookId);

                if (lorebook == null)
                {
                    LoggingManager.LogToFile("a0392d07-cd2a-4caa-8a04-3b7fa099d88d", $"Lorebook [{lorebookDbModel.LorebookId}] to update wasn't found in storage.");
                    return false;
                }

                lorebook.LastActivityAtUtc = DateTime.UtcNow;

                // Only handle overridable fields
                lorebook.Name = lorebookDbModel.Name;
                lorebook.Entries = lorebookDbModel.Entries;

                EntityEntry<LorebookDbModel> result = dbContext.Lorebooks.Update(lorebook);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("13ebc049-c036-4c6d-81ad-473c2065c92d", $"Error when updating Lorebook. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(lorebookDbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("59607515-b5ca-4dab-973b-0f89d202b3d0", $"Error when querying pending queries on table Lorebooks.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteLorebookAsync(LorebookDbModel lorebookDbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var lorebook = dbContext.Lorebooks.FirstOrDefault(w => w.LorebookId == lorebookDbModel.LorebookId);

                if (lorebook == null)
                {
                    LoggingManager.LogToFile("56834d39-036e-4b7d-b53c-8dae96b2e2cb", $"Lorebook [{lorebookDbModel.LorebookId}] to update wasn't found in storage.");
                    return false;
                }

                EntityEntry<LorebookDbModel> result = dbContext.Lorebooks.Remove(lorebook);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("a5cc2d55-c222-42e7-9b32-0de80846771f", $"Error when deleting a Lorebook. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(lorebookDbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("1ad84bd3-adde-4368-a2ad-4ba44c0814e0", $"Error when querying pending queries on table Lorebooks.", ex);
                return false;
            }
        }
    }
}
