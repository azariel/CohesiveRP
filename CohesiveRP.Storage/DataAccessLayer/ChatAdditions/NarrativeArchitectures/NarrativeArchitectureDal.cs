using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeArchitecture
{
    /// <summary>
    /// DataAccessLayer around NarrativeArchitectures.
    /// </summary>
    public class NarrativeArchitectureDal : StorageDal, INarrativeArchitecturesDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public NarrativeArchitectureDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<NarrativeArchitectureDbModel> AddNarrativeArchitectureAsync(NarrativeArchitectureDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Check if NarrativeArchitecture for this chat already exist
                var NarrativeArchitecture = dbContext.NarrativeArchitectures.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (NarrativeArchitecture != null)
                {
                    LoggingManager.LogToFile("79813e58-7c6c-4dcc-94f7-0555087d307f", $"Error when querying Db on table cohesion enforcements. NarrativeArchitecture entity to create already exists with Id [{NarrativeArchitecture.NarrativeArchitectureId}].");
                    return null;
                }

                // Override system fields
                queryModel.NarrativeArchitectureId ??= Guid.NewGuid().ToString();
                queryModel.CreatedAtUtc = DateTime.UtcNow;

                // Create the NarrativeArchitecture row tied to this chat
                EntityEntry<NarrativeArchitectureDbModel> resultAdd = dbContext.NarrativeArchitectures.Add(queryModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("15eb9530-f8b5-405c-afd8-275188781dc1", $"Error when querying Db on table NarrativeArchitectures. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return queryModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ec01037c-46fa-42b6-9339-e6e0df93cff4", $"Error when querying Db on table NarrativeArchitectures.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteNarrativeArchitectureAsync(Func<NarrativeArchitectureDbModel, bool> func)
        {
            if (func == null)
            {
                return true;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var items = await dbContext.NarrativeArchitectures.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();

                if (items == null || items.Length <= 0)
                {
                    LoggingManager.LogToFile("3cddaeca-9dc7-4840-b93e-e0dda39dfb80", $"NarrativeArchitectures tied to Func [{func}] to delete weren't found in storage.");
                    return false;
                }

                foreach (var item in items)
                {
                    var result = dbContext.NarrativeArchitectures.Remove(item);
                    if (result.State != EntityState.Deleted)
                    {
                        LoggingManager.LogToFile("f433a2a2-51ea-40dc-a0be-6ee47568885a", $"Error when deleting a specific NarrativeArchitecture [{item}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    }
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("266cc4b2-786e-42a2-9af7-3160dbd5d4c6", $"Error when querying queries on table NarrativeArchitectures.", ex);
                return false;
            }
        }

        public async Task<NarrativeArchitectureDbModel[]> GetNarrativeArchitecturesAsync(Func<NarrativeArchitectureDbModel, bool> func)
        {
             try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if (func == null)
                    return dbContext.NarrativeArchitectures.ToArray();

                var result = await dbContext.NarrativeArchitectures.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("4dba4157-7035-43ab-88bd-a870dfe29776", $"Error when querying Db on table NarrativeArchitectures.", ex);
                return null;
            }
        }

        public async Task<NarrativeArchitectureDbModel> UpdateNarrativeArchitectureAsync(NarrativeArchitectureDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.NarrativeArchitectures.AsNoTracking().FirstOrDefault(w => w.NarrativeArchitectureId == dbModel.NarrativeArchitectureId);

                if (item == null)
                {
                    LoggingManager.LogToFile("16dad879-5158-48e6-a625-6a5d16e191e8", $"NarrativeArchitectures [{dbModel.NarrativeArchitectureId}] to update wasn't found in storage.");
                    return null;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.ChatId = item.ChatId;

                var result = dbContext.NarrativeArchitectures.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("15c77d5e-3070-4c11-8e5e-dd705ba5d984", $"Error when updating NarrativeArchitectures. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("01db28aa-fa55-480f-8563-cf9e0d575c14", $"Error when querying queries on table NarrativeArchitectures.", ex);
                return null;
            }
        }
    }
}
