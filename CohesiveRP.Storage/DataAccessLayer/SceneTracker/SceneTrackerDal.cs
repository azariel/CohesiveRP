using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker;
using CohesiveRP.Storage.QueryModels.SceneTracker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around SceneTrackers.
    /// </summary>
    public class SceneTrackerDal : StorageDal, ISceneTrackerDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public SceneTrackerDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<SceneTrackerDbModel> AddSceneTracker(CreateSceneTrackerQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Convert model
                var dbModel = new SceneTrackerDbModel
                {
                    SceneTrackerId = Guid.NewGuid().ToString(),
                    ChatId = queryModel.ChatId,
                    CreatedAtUtc = queryModel.CreatedAtUtc,
                    LinkMessageId = queryModel.LinkMessageId,
                    Content = queryModel.Content,
                };

                // Check if SceneTrackers for this chat already exist
                var sceneTracker = dbContext.SceneTrackers.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (sceneTracker != null)
                {
                    LoggingManager.LogToFile("01502b25-a69d-4e1b-9426-5c618556c906", $"Error when querying Db on table scene trackers. SceneTracker entity to create already exists [{sceneTracker.SceneTrackerId}].");
                    return null;
                }

                // Create the sceneTracker row tied to this chat
                EntityEntry<SceneTrackerDbModel> resultAdd = dbContext.SceneTrackers.Add(dbModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("b0fe94b7-bbff-423b-9817-02b3f7825e2f", $"Error when querying Db on table sceneTrackers. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("9cee96e8-993e-46c2-ad8a-9ffb9f8de830", $"Error when querying Db on table sceneTrackers.", ex);
                return null;
            }
        }

        public async Task<SceneTrackerDbModel> GetSceneTracker(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.SceneTrackers.FirstOrDefault(w => w.ChatId == chatId);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ca8f953f-cbea-468e-b74e-44badc6d2287", $"Error when querying Db on table SceneTrackers.", ex);
                return null;
            }
        }

        public async Task<SceneTrackerDbModel> CreateOrUpdateSceneTracker(CreateSceneTrackerQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Constant fields not to update
                var sceneTracker = dbContext.SceneTrackers.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (sceneTracker == null)
                {
                    // Create the sceneTracker row tied to this chat first
                    var sceneTrackerObj = new SceneTrackerDbModel
                    {
                        SceneTrackerId = Guid.NewGuid().ToString(),
                        ChatId = queryModel.ChatId,
                        LinkMessageId = queryModel.LinkMessageId,
                        CreatedAtUtc = DateTime.UtcNow,
                        Content = queryModel.Content,
                    };

                    EntityEntry<SceneTrackerDbModel> resultAdd = dbContext.SceneTrackers.Add(sceneTrackerObj);
                    if (resultAdd.State != EntityState.Added)
                    {
                        LoggingManager.LogToFile("c1de50a0-aadd-4cbc-9527-6dd70b49d7f0", $"Error when querying Db on table sceneTrackers. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                        return null;
                    }

                    await dbContext.SaveChangesAsync();
                    return resultAdd.Entity;
                }

                // Those are the TWO accepted fields to update
                sceneTracker.Content = queryModel.Content;
                sceneTracker.LinkMessageId = queryModel.LinkMessageId;

                EntityEntry<SceneTrackerDbModel> result = dbContext.SceneTrackers.Update(sceneTracker);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("3ed161d1-da66-4081-9711-4c9fda87bfcf", $"Error when updating Dbmodel on table sceneTrackers. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. sceneTrackerObj: [{JsonCommonSerializer.SerializeToString(sceneTracker)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("c1fb1516-beb5-42c0-9bd8-b64b229a86f2", $"Error when querying pending queries on table sceneTrackers.", ex);
                return null;
            }
        }
    }
}
