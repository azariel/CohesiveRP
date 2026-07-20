using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.CohesionEnforcement
{
    /// <summary>
    /// DataAccessLayer around CohesionEnforcements.
    /// </summary>
    public class CohesionEnforcementDal : StorageDal, ICohesionEnforcementsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public CohesionEnforcementDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<CohesionEnforcementDbModel> AddCohesionEnforcementAsync(CohesionEnforcementDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Check if CohesionEnforcement for this chat already exist
                var cohesionEnforcement = dbContext.CohesionEnforcements.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (cohesionEnforcement != null)
                {
                    LoggingManager.LogToFile("23d529e8-b080-4d6a-849a-9830f40f8f6d", $"Error when querying Db on table cohesion enforcements. CohesionEnforcement entity to create already exists with Id [{cohesionEnforcement.CohesionEnforcementId}].");
                    return null;
                }

                // Override system fields
                queryModel.CohesionEnforcementId ??= Guid.NewGuid().ToString();
                queryModel.CreatedAtUtc = DateTime.UtcNow;

                // Create the CohesionEnforcement row tied to this chat
                EntityEntry<CohesionEnforcementDbModel> resultAdd = dbContext.CohesionEnforcements.Add(queryModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("009e00ed-fbaa-4a3b-bc7e-2f2c85076a11", $"Error when querying Db on table CohesionEnforcements. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return queryModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("800f9fab-5332-49fd-8fa5-f16cf5e8d41a", $"Error when querying Db on table CohesionEnforcements.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteCohesionEnforcementAsync(Func<CohesionEnforcementDbModel, bool> func)
        {
            if (func == null)
            {
                return true;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var items = await dbContext.CohesionEnforcements.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();

                if (items == null || items.Length <= 0)
                {
                    LoggingManager.LogToFile("296b34bd-e90d-4747-9f72-1fa97bfb25d3", $"CohesionEnforcements tied to Func [{func}] to delete weren't found in storage.");
                    return false;
                }

                foreach (var item in items)
                {
                    var result = dbContext.CohesionEnforcements.Remove(item);
                    if (result.State != EntityState.Deleted)
                    {
                        LoggingManager.LogToFile("f208c199-cf44-470c-8237-aea2b3e41604", $"Error when deleting a specific CohesionEnforcement [{item}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    }
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("11e0f24a-a06c-4d4a-8c13-b844fb0d6dbe", $"Error when querying queries on table CohesionEnforcements.", ex);
                return false;
            }
        }

        public async Task<CohesionEnforcementDbModel[]> GetCohesionEnforcementsAsync(Func<CohesionEnforcementDbModel, bool> func)
        {
             try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if (func == null)
                    return dbContext.CohesionEnforcements.ToArray();

                var result = await dbContext.CohesionEnforcements.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("fbc9a42b-aea8-4911-8536-4ce24a870391", $"Error when querying Db on table CohesionEnforcements.", ex);
                return null;
            }
        }

        public async Task<CohesionEnforcementDbModel> UpdateCohesionEnforcementAsync(CohesionEnforcementDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.CohesionEnforcements.AsNoTracking().FirstOrDefault(w => w.CohesionEnforcementId == dbModel.CohesionEnforcementId);

                if (item == null)
                {
                    LoggingManager.LogToFile("81c0d548-862b-4be5-bebe-05ba306f4b63", $"CohesionEnforcements [{dbModel.CohesionEnforcementId}] to update wasn't found in storage.");
                    return null;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.ChatId = item.ChatId;

                var result = dbContext.CohesionEnforcements.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("6ea6abf5-aa86-4762-9f57-99350b6ce912", $"Error when updating CohesionEnforcements. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("1d3f65a8-b4d1-4d08-bec1-39cf38a8ab3e", $"Error when querying queries on table CohesionEnforcements.", ex);
                return null;
            }
        }
    }
}
