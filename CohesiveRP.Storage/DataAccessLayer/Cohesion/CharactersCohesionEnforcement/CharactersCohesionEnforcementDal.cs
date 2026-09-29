using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement
{
    /// <summary>
    /// DataAccessLayer around CharactersCohesionEnforcementDal.
    /// </summary>
    public class CharactersCohesionEnforcementDal : StorageDal, ICharactersCohesionEnforcementsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public CharactersCohesionEnforcementDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<CharactersCohesionEnforcementDbModel> AddCharactersCohesionEnforcementAsync(CharactersCohesionEnforcementDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Check if CohesionEnforcement for this chat already exist
                var cohesionEnforcement = dbContext.CharactersCohesionEnforcements.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (cohesionEnforcement != null)
                {
                    LoggingManager.LogToFile("23d529e8-b080-4d6a-849a-9830f40f8f6d", $"Error when querying Db on table characters cohesion enforcements. CharactersCohesionEnforcement entity to create already exists with Id [{cohesionEnforcement.CharactersCohesionEnforcementId}].");
                    return null;
                }

                // Override system fields
                queryModel.CharactersCohesionEnforcementId ??= Guid.NewGuid().ToString();
                queryModel.CreatedAtUtc = DateTime.UtcNow;

                // Create the CohesionEnforcement row tied to this chat
                EntityEntry<CharactersCohesionEnforcementDbModel> resultAdd = dbContext.CharactersCohesionEnforcements.Add(queryModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("009e00ed-fbaa-4a3b-bc7e-2f2c85076a11", $"Error when querying Db on table CharactersCohesionEnforcements. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return queryModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("800f9fab-5332-49fd-8fa5-f16cf5e8d41a", $"Error when querying Db on table CharactersCohesionEnforcements.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteCharactersCohesionEnforcementAsync(Func<CharactersCohesionEnforcementDbModel, bool> func)
        {
            if (func == null)
            {
                return true;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var items = await dbContext.CharactersCohesionEnforcements.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();

                if (items == null || items.Length <= 0)
                {
                    LoggingManager.LogToFile("296b34bd-e90d-4747-9f72-1fa97bfb25d3", $"CharactersCohesionEnforcements tied to Func [{func}] to delete weren't found in storage.");
                    return false;
                }

                foreach (var item in items)
                {
                    var result = dbContext.CharactersCohesionEnforcements.Remove(item);
                    if (result.State != EntityState.Deleted)
                    {
                        LoggingManager.LogToFile("f208c199-cf44-470c-8237-aea2b3e41604", $"Error when deleting a specific CohesionEnforcement [{item}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    }
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("11e0f24a-a06c-4d4a-8c13-b844fb0d6dbe", $"Error when querying queries on table CharactersCohesionEnforcements.", ex);
                return false;
            }
        }

        public async Task<CharactersCohesionEnforcementDbModel[]> GetCharactersCohesionEnforcementsAsync(Func<CharactersCohesionEnforcementDbModel, bool> func)
        {
             try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if (func == null)
                    return dbContext.CharactersCohesionEnforcements.ToArray();

                var result = await dbContext.CharactersCohesionEnforcements.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("fbc9a42b-aea8-4911-8536-4ce24a870391", $"Error when querying Db on table CharactersCohesionEnforcements.", ex);
                return null;
            }
        }

        public async Task<CharactersCohesionEnforcementDbModel> UpdateCharactersCohesionEnforcementAsync(CharactersCohesionEnforcementDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.CharactersCohesionEnforcements.AsNoTracking().FirstOrDefault(w => w.CharactersCohesionEnforcementId == dbModel.CharactersCohesionEnforcementId);

                if (item == null)
                {
                    LoggingManager.LogToFile("81c0d548-862b-4be5-bebe-05ba306f4b63", $"CharactersCohesionEnforcements [{dbModel.CharactersCohesionEnforcementId}] to update wasn't found in storage.");
                    return null;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.ChatId = item.ChatId;

                var result = dbContext.CharactersCohesionEnforcements.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("6ea6abf5-aa86-4762-9f57-99350b6ce912", $"Error when updating CharactersCohesionEnforcements. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("1d3f65a8-b4d1-4d08-bec1-39cf38a8ab3e", $"Error when querying queries on table CharactersCohesionEnforcements.", ex);
                return null;
            }
        }
    }
}
