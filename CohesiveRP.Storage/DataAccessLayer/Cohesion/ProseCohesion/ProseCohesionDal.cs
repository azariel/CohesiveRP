using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.ProseCohesion
{
    /// <summary>
    /// DataAccessLayer around ProseCohesion.
    /// </summary>
    public class ProseCohesionDal : StorageDal, IProseCohesionDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public ProseCohesionDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<ProseCohesionDbModel> AddProseCohesionAsync(ProseCohesionDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Check if CohesionEnforcement for this chat already exist
                var cohesionEnforcement = dbContext.ProseCohesions.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (cohesionEnforcement != null)
                {
                    LoggingManager.LogToFile("aeb951af-724d-4a25-82f0-35c4fb32e435", $"Error when querying Db on table proseCohesions. ProseCohesion entity to create already exists with Id [{cohesionEnforcement.ProseCohesionId}].");
                    return null;
                }

                // Override system fields
                queryModel.ProseCohesionId ??= Guid.NewGuid().ToString();
                queryModel.CreatedAtUtc = DateTime.UtcNow;

                // Create the CohesionEnforcement row tied to this chat
                EntityEntry<ProseCohesionDbModel> resultAdd = dbContext.ProseCohesions.Add(queryModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("1473ab7c-d65e-480e-ae4e-8d5f92f1505f", $"Error when querying Db on table ProseCohesions. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return queryModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("2fbdbacc-f783-4efe-a066-44badbe9e1db", $"Error when querying Db on table ProseCohesions.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteProseCohesionAsync(Func<ProseCohesionDbModel, bool> func)
        {
            if (func == null)
            {
                return true;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var items = await dbContext.ProseCohesions.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();

                if (items == null || items.Length <= 0)
                {
                    LoggingManager.LogToFile("f4f8cfa6-404e-4f76-8639-019a56be513d", $"ProseCohesions tied to Func [{func}] to delete weren't found in storage.");
                    return false;
                }

                foreach (var item in items)
                {
                    var result = dbContext.ProseCohesions.Remove(item);
                    if (result.State != EntityState.Deleted)
                    {
                        LoggingManager.LogToFile("3d03e03a-8ce5-4fcd-b493-0daf63cf5aa7", $"Error when deleting a specific proseCohesion [{item}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    }
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("cb3d3edc-bc8d-4ca5-a30e-9ebbac44de10", $"Error when querying queries on table ProseCohesions.", ex);
                return false;
            }
        }

        public async Task<ProseCohesionDbModel[]> GetProseCohesionsAsync(Func<ProseCohesionDbModel, bool> func)
        {
             try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if (func == null)
                    return dbContext.ProseCohesions.ToArray();

                var result = await dbContext.ProseCohesions.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("030e6167-2918-45e8-9c57-4409a97360fc", $"Error when querying Db on table ProseCohesions.", ex);
                return null;
            }
        }

        public async Task<ProseCohesionDbModel> UpdateProseCohesionAsync(ProseCohesionDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.ProseCohesions.AsNoTracking().FirstOrDefault(w => w.ProseCohesionId == dbModel.ProseCohesionId);

                if (item == null)
                {
                    LoggingManager.LogToFile("8160cd9a-5d4e-400f-82ce-bf7d827bd244", $"ProseCohesions [{dbModel.ProseCohesionId}] to update wasn't found in storage.");
                    return null;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.ChatId = item.ChatId;

                var result = dbContext.ProseCohesions.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("afe7cbe0-6c64-4181-8130-8211a658cc86", $"Error when updating ProseCohesions. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("37a76bd2-2380-4249-bea9-c60dad91d1fd", $"Error when querying queries on table ProseCohesions.", ex);
                return null;
            }
        }
    }
}
