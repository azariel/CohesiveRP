using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.StyleCohesion
{
    /// <summary>
    /// DataAccessLayer around StyleCohesionDal.
    /// </summary>
    public class StyleCohesionDal : StorageDal, IStyleCohesionsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public StyleCohesionDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<StyleCohesionDbModel> AddStyleCohesionAsync(StyleCohesionDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Check if StyleCohesion for this chat already exist
                var StyleCohesion = dbContext.StyleCohesions.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (StyleCohesion != null)
                {
                    LoggingManager.LogToFile("d07296ba-95e5-4aa7-8ffd-8be73fd6e6ab", $"Error when querying Db on table characters StyleCohesions. StyleCohesion entity to create already exists with Id [{StyleCohesion.StyleCohesionId}].");
                    return null;
                }

                // Override system fields
                queryModel.StyleCohesionId ??= Guid.NewGuid().ToString();
                queryModel.CreatedAtUtc = DateTime.UtcNow;

                // Create the StyleCohesion row tied to this chat
                EntityEntry<StyleCohesionDbModel> resultAdd = dbContext.StyleCohesions.Add(queryModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("7e6789e3-bf26-4e44-bf4f-e8253957c22c", $"Error when querying Db on table StyleCohesions. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return queryModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("9e5e3fb2-833c-43c3-b151-115eb5a6276d", $"Error when querying Db on table StyleCohesions.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteStyleCohesionAsync(Func<StyleCohesionDbModel, bool> func)
        {
            if (func == null)
            {
                return true;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var items = await dbContext.StyleCohesions.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();

                if (items == null || items.Length <= 0)
                {
                    LoggingManager.LogToFile("958d75ad-7319-40c2-b49e-f9229374a549", $"StyleCohesions tied to Func [{func}] to delete weren't found in storage.");
                    return false;
                }

                foreach (var item in items)
                {
                    var result = dbContext.StyleCohesions.Remove(item);
                    if (result.State != EntityState.Deleted)
                    {
                        LoggingManager.LogToFile("e7bf46db-7677-4d54-a24d-a2758b53468a", $"Error when deleting a specific StyleCohesion [{item}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    }
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("8154dd7a-b257-471f-aae0-524ce85a744bS", $"Error when querying queries on table StyleCohesions.", ex);
                return false;
            }
        }

        public async Task<StyleCohesionDbModel[]> GetStyleCohesionsAsync(Func<StyleCohesionDbModel, bool> func)
        {
             try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if (func == null)
                    return dbContext.StyleCohesions.ToArray();

                var result = await dbContext.StyleCohesions.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("a14bb938-f3b2-4098-8c31-3b0994991627", $"Error when querying Db on table StyleCohesions.", ex);
                return null;
            }
        }

        public async Task<StyleCohesionDbModel> UpdateStyleCohesionAsync(StyleCohesionDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.StyleCohesions.AsNoTracking().FirstOrDefault(w => w.StyleCohesionId == dbModel.StyleCohesionId);

                if (item == null)
                {
                    LoggingManager.LogToFile("9e3b4542-967d-4c19-bb78-90740cd80e48", $"StyleCohesions [{dbModel.StyleCohesionId}] to update wasn't found in storage.");
                    return null;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.ChatId = item.ChatId;

                var result = dbContext.StyleCohesions.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("be7e03c8-6b63-40a3-8b16-7ef9d3548031", $"Error when updating StyleCohesions. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("b6ba7d34-7bd6-4b77-936c-c60d3c8d161c", $"Error when querying queries on table StyleCohesions.", ex);
                return null;
            }
        }
    }
}
