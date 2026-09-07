using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.Reflection
{
    /// <summary>
    /// DataAccessLayer around Reflections.
    /// </summary>
    public class ReflectionDal : StorageDal, IReflectionsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public ReflectionDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<ReflectionDbModel> AddReflectionAsync(ReflectionDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Check if Reflection for this chat already exist
                var Reflection = dbContext.Reflections.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (Reflection != null)
                {
                    LoggingManager.LogToFile("b170a3c5-9451-4a7d-b28b-da3b7d6c8dd2", $"Error when querying Db on table cohesion enforcements. Reflection entity to create already exists with Id [{Reflection.ReflectionId}].");
                    return null;
                }

                // Override system fields
                queryModel.ReflectionId ??= Guid.NewGuid().ToString();
                queryModel.CreatedAtUtc = DateTime.UtcNow;

                // Create the Reflection row tied to this chat
                EntityEntry<ReflectionDbModel> resultAdd = dbContext.Reflections.Add(queryModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("ecf9345d-e107-43e0-8466-d4b4241fd727", $"Error when querying Db on table Reflections. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return queryModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("7f7ff507-cabd-4c85-b297-d45165e42aef", $"Error when querying Db on table Reflections.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteReflectionAsync(Func<ReflectionDbModel, bool> func)
        {
            if (func == null)
            {
                return true;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var items = await dbContext.Reflections.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();

                if (items == null || items.Length <= 0)
                {
                    LoggingManager.LogToFile("a48f25f7-793c-4010-8c6e-f8915801b871", $"Reflections tied to Func [{func?.Method}, {func?.Target}] to delete weren't found in storage.");
                    return false;
                }

                foreach (var item in items)
                {
                    var result = dbContext.Reflections.Remove(item);
                    if (result.State != EntityState.Deleted)
                    {
                        LoggingManager.LogToFile("143d0b96-aa72-4e72-b180-3d475ac95453", $"Error when deleting a specific Reflection [{item}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    }
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("8951b38c-ba46-4ca0-91b9-2eff9da05400", $"Error when querying queries on table Reflections.", ex);
                return false;
            }
        }

        public async Task<ReflectionDbModel[]> GetReflectionsAsync(Func<ReflectionDbModel, bool> func)
        {
             try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if (func == null)
                    return dbContext.Reflections.ToArray();

                var result = await dbContext.Reflections.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("f234ba27-7056-4171-a605-e24e6fb7b3c4", $"Error when querying Db on table Reflections.", ex);
                return null;
            }
        }

        public async Task<ReflectionDbModel> UpdateReflectionAsync(ReflectionDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.Reflections.AsNoTracking().FirstOrDefault(w => w.ReflectionId == dbModel.ReflectionId);

                if (item == null)
                {
                    LoggingManager.LogToFile("b90a0249-15fc-4533-aa66-0cd6c2344485", $"Reflections [{dbModel.ReflectionId}] to update wasn't found in storage.");
                    return null;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.ChatId = item.ChatId;

                var result = dbContext.Reflections.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("cbc99eca-22a7-459f-970b-d9135b80d944", $"Error when updating Reflections. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("5e51dbec-feb2-4159-8782-3e2fa807bbf4", $"Error when querying queries on table Reflections.", ex);
                return null;
            }
        }
    }
}
