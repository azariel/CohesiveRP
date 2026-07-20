using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Global Settings.
    /// </summary>
    public class LLMApiQueriesDal : StorageDal, ILLMApiQueriesDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public LLMApiQueriesDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;
            Cleanup();
        }

        private void Cleanup()
        {
            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();

            // Always clear ALL rows in this table upon startup
            dbContext.LLMApiQueries.ExecuteDelete();
            dbContext.SaveChanges();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<LLMApiQueryDbModel[]> GetQueriesOnLLMApisAsync(string tag)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if(string.IsNullOrWhiteSpace(tag))
                    return dbContext.LLMApiQueries.ToArray();

                return dbContext.LLMApiQueries.Where(w => w.Tag == tag).ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("fff66323-3ddb-4ff9-b51c-3c118813e166", $"Error when getting query by tag [{tag}] on table LLMApiQueries.", ex);
                return null;
            }
        }

        public async Task<LLMApiQueryDbModel> AddNewQueryAsync(LLMApiQueryDbModel newQuery)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                var dbModel = new LLMApiQueryDbModel
                {
                    LLMApiQueryId = newQuery.LLMApiQueryId ?? Guid.NewGuid().ToString(),
                    LLMProviderConfigId = newQuery.LLMProviderConfigId,
                    CreatedAtUtc = DateTime.UtcNow,
                    Status = newQuery.Status,
                    Tag = newQuery.Tag,
                };

                EntityEntry<LLMApiQueryDbModel> result = await dbContext.LLMApiQueries.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("19803d68-e82d-41b9-afea-361a917802b6", $"Error when querying Db on table LLMApiQueries. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("2298fc84-6605-4040-8ca0-4b77b5bf800c", $"Error when adding new query on table LLMApiQueries.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteQueryByIdAsync(string llmApiQueryId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                LLMApiQueryDbModel[] queriesToRemove = dbContext.LLMApiQueries.Where(w => w.LLMApiQueryId == llmApiQueryId).ToArray();

                foreach (var item in queriesToRemove)
                {
                    var result = dbContext.LLMApiQueries.Remove(item);

                    if (result.State != EntityState.Deleted)
                    {
                        LoggingManager.LogToFile("23d6d93b-3f8b-422d-81a9-0dac03dead3f", $"Error when deleting a query on table LLMApiQueries. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. Aborting.");
                        return false;
                    }
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("4e6e4272-f8a6-487b-8367-a43c4f67b10e", $"Error when deleting LLMApiQuery by Id [{llmApiQueryId}] on table LLMApiQueries.", ex);
                return false;
            }
        }
    }
}
