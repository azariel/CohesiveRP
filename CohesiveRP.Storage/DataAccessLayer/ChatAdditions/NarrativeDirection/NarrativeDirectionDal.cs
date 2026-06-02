using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeDirection
{
    /// <summary>
    /// DataAccessLayer around NarrativeDirections.
    /// </summary>
    public class NarrativeDirectionDal : StorageDal, INarrativeDirectionsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public NarrativeDirectionDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<NarrativeDirectionDbModel> AddNarrativeDirectionAsync(NarrativeDirectionDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Check if NarrativeDirection for this chat already exist
                var NarrativeDirection = dbContext.NarrativeDirections.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (NarrativeDirection != null)
                {
                    LoggingManager.LogToFile("80b967e3-5778-422f-b05d-da3a392a4a10", $"Error when querying Db on table cohesion enforcements. NarrativeDirection entity to create already exists with Id [{NarrativeDirection.NarrativeDirectionId}].");
                    return null;
                }

                // Override system fields
                queryModel.NarrativeDirectionId ??= Guid.NewGuid().ToString();
                queryModel.CreatedAtUtc = DateTime.UtcNow;

                // Create the NarrativeDirection row tied to this chat
                EntityEntry<NarrativeDirectionDbModel> resultAdd = dbContext.NarrativeDirections.Add(queryModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("8160de37-5be7-49d2-b841-0d83b47162a6", $"Error when querying Db on table NarrativeDirections. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return queryModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("b2703c56-da5d-43cb-a645-98bd960d8310", $"Error when querying Db on table NarrativeDirections.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteNarrativeDirectionAsync(Func<NarrativeDirectionDbModel, bool> func)
        {
            if (func == null)
            {
                return true;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var items = await dbContext.NarrativeDirections.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();

                if (items == null || items.Length <= 0)
                {
                    LoggingManager.LogToFile("a9345f28-dbd9-4e75-a693-1ca68e862bd1", $"NarrativeDirections tied to Func [{func}] to delete weren't found in storage.");
                    return false;
                }

                foreach (var item in items)
                {
                    var result = dbContext.NarrativeDirections.Remove(item);
                    if (result.State != EntityState.Deleted)
                    {
                        LoggingManager.LogToFile("17c297b3-81c4-4a6a-9ec2-7c9e39d6b3e0", $"Error when deleting a specific NarrativeDirection [{item}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    }
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("0c955578-178f-46f5-b137-25c1fdd1572d", $"Error when querying queries on table NarrativeDirections.", ex);
                return false;
            }
        }

        public async Task<NarrativeDirectionDbModel[]> GetNarrativeDirectionsAsync(Func<NarrativeDirectionDbModel, bool> func)
        {
             try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                if (func == null)
                    return dbContext.NarrativeDirections.ToArray();

                var result = await dbContext.NarrativeDirections.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("c2e96d3b-81a2-4b75-9a88-f3745f4dd1a1", $"Error when querying Db on table NarrativeDirections.", ex);
                return null;
            }
        }

        public async Task<NarrativeDirectionDbModel> UpdateNarrativeDirectionAsync(NarrativeDirectionDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var item = dbContext.NarrativeDirections.AsNoTracking().FirstOrDefault(w => w.NarrativeDirectionId == dbModel.NarrativeDirectionId);

                if (item == null)
                {
                    LoggingManager.LogToFile("413c981f-840d-4d2f-bd80-c5b3f011360f", $"NarrativeDirections [{dbModel.NarrativeDirectionId}] to update wasn't found in storage.");
                    return null;
                }

                // Force set the system, unmodifiable fields to avoid any unwanted changes
                dbModel.CreatedAtUtc = item.CreatedAtUtc;
                dbModel.ChatId = item.ChatId;

                var result = dbContext.NarrativeDirections.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("3ce495eb-c1c3-41dc-b008-791541b7746a", $"Error when updating NarrativeDirections. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("de6fb5ac-34da-4eba-b276-c56b732f9808", $"Error when querying queries on table NarrativeDirections.", ex);
                return null;
            }
        }
    }
}
