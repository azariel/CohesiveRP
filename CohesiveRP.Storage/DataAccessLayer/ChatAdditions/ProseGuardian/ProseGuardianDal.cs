using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.ProseGuardian
{
    /// <summary>
    /// DataAccessLayer around ProseGuardians.
    /// </summary>
    public class ProseGuardianDal : StorageDal, IProseGuardiansDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public ProseGuardianDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<ProseGuardianDbModel> AddProseGuardianAsync(ProseGuardianDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Check if ProseGuardian for this chat already exist
                var ProseGuardian = dbContext.ProseGuardians.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (ProseGuardian != null)
                {
                    LoggingManager.LogToFile("4b32f5d6-9512-41e3-af80-2228f067b3f5", $"Error when querying Db on table cohesion enforcements. ProseGuardian entity to create already exists with Id [{ProseGuardian.ProseGuardianId}].");
                    return null;
                }

                // Override system fields
                queryModel.ProseGuardianId ??= Guid.NewGuid().ToString();
                queryModel.CreatedAtUtc = DateTime.UtcNow;

                // Create the ProseGuardian row tied to this chat
                EntityEntry<ProseGuardianDbModel> resultAdd = dbContext.ProseGuardians.Add(queryModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("d602517e-eac1-45a2-bbac-9826778cf65b", $"Error when querying Db on table ProseGuardians. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return queryModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("36660d1e-bf3d-46c2-b5e4-132f5449819d", $"Error when querying Db on table ProseGuardians.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteProseGuardianAsync(Func<ProseGuardianDbModel, bool> func)
        {
            if (func == null)
            {
                return true;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var items = await dbContext.ProseGuardians.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();

                if (items == null || items.Length <= 0)
                {
                    LoggingManager.LogToFile("f3e6f2fc-2b16-4be8-a6c4-60b6b99daf97", $"ProseGuardians tied to Func [{func}] to delete weren't found in storage.");
                    return false;
                }

                foreach (var item in items)
                {
                    var result = dbContext.ProseGuardians.Remove(item);
                    if (result.State != EntityState.Deleted)
                    {
                        LoggingManager.LogToFile("07a59ee7-d0ad-4012-a2fd-7674dff82aa4", $"Error when deleting a specific ProseGuardian [{item}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    }
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("adaf57c3-3899-44bb-bdeb-98bd3e50ca99", $"Error when querying queries on table ProseGuardians.", ex);
                return false;
            }
        }

        public async Task<ProseGuardianDbModel[]> GetProseGuardiansAsync(Func<ProseGuardianDbModel, bool> func)
        {
             try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if (func == null)
                    return dbContext.ProseGuardians.ToArray();

                var result = await dbContext.ProseGuardians.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("438f3e8d-9d89-4656-9c45-e4b7db00cde4", $"Error when querying Db on table ProseGuardians.", ex);
                return null;
            }
        }

        public async Task<ProseGuardianDbModel> UpdateProseGuardianAsync(ProseGuardianDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.ProseGuardians.AsNoTracking().FirstOrDefault(w => w.ProseGuardianId == dbModel.ProseGuardianId);

                if (item == null)
                {
                    LoggingManager.LogToFile("4c707303-4f42-4762-86f0-d04db443940e", $"ProseGuardians [{dbModel.ProseGuardianId}] to update wasn't found in storage.");
                    return null;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.ChatId = item.ChatId;

                var result = dbContext.ProseGuardians.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("2b7fcaa9-f7c1-4ba1-b8cd-534b7e7d29da", $"Error when updating ProseGuardians. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("d9ff5985-216f-4b5f-a609-c1e12e05f063", $"Error when querying queries on table ProseGuardians.", ex);
                return null;
            }
        }
    }
}
