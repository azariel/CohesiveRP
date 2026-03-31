using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around SceneAnalyzers.
    /// </summary>
    public class SceneAnalyzerDal : StorageDal, ISceneAnalyzerDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public SceneAnalyzerDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<SceneAnalyzerDbModel> AddSceneAnalyzerAsync(SceneAnalyzerDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Convert model
                var dbModel = new SceneAnalyzerDbModel
                {
                    SceneAnalyzerId = Guid.NewGuid().ToString(),
                    CreatedAtUtc = DateTime.UtcNow,
                    ChatId = queryModel.ChatId,
                    LinkedMessageId = queryModel.LinkedMessageId,
                };

                // Check if SceneAnalyzers for this chat already exist
                var sceneAnalyzer = dbContext.SceneAnalyzers.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (sceneAnalyzer != null)
                {
                    LoggingManager.LogToFile("257a4623-46b7-4630-8356-75aee3f28e16", $"Error when querying Db on table scene analyzers. SceneAnalyzer entity to create already exists [{sceneAnalyzer.SceneAnalyzerId}].");
                    return null;
                }

                // Create the sceneAnalyzer row tied to this chat
                EntityEntry<SceneAnalyzerDbModel> resultAdd = dbContext.SceneAnalyzers.Add(dbModel);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("c20d9640-73a2-4bad-ad1a-7c14a1da38f9", $"Error when querying Db on table sceneAnalyzers. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return dbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("bc40805f-0aee-46fe-a31a-6a428e94a4f7", $"Error when querying Db on table sceneAnalyzers.", ex);
                return null;
            }
        }

        public async Task<SceneAnalyzerDbModel> GetSceneAnalyzerAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.SceneAnalyzers.FirstOrDefault(w => w.ChatId == chatId);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("1513f239-43cc-4c31-83dd-67a243590dee", $"Error when querying Db on table SceneAnalyzers.", ex);
                return null;
            }
        }

        public async Task<SceneAnalyzerDbModel> CreateOrUpdateSceneAnalyzerAsync(SceneAnalyzerDbModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Constant fields not to update
                var sceneAnalyzer = dbContext.SceneAnalyzers.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (sceneAnalyzer == null)
                {
                    // Create the sceneAnalyzer row tied to this chat first
                    var sceneAnalyzerObj = new SceneAnalyzerDbModel
                    {
                        SceneAnalyzerId = Guid.NewGuid().ToString(),
                        CreatedAtUtc = DateTime.UtcNow,
                        ChatId = queryModel.ChatId,
                        LinkedMessageId = queryModel.LinkedMessageId,
                    };

                    EntityEntry<SceneAnalyzerDbModel> resultAdd = dbContext.SceneAnalyzers.Add(sceneAnalyzerObj);
                    if (resultAdd.State != EntityState.Added)
                    {
                        LoggingManager.LogToFile("5f3720f0-2849-4048-b4de-e58cd7b1fb7f", $"Error when querying Db on table sceneAnalyzers. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                        return null;
                    }

                    await dbContext.SaveChangesAsync();
                    return resultAdd.Entity;
                }

                // Those are the accepted fields to update
                sceneAnalyzer.LinkedMessageId = queryModel.LinkedMessageId;

                EntityEntry<SceneAnalyzerDbModel> result = dbContext.SceneAnalyzers.Update(sceneAnalyzer);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("7ff5a56e-1dd6-407b-bf6f-975fc5dc3de6", $"Error when updating Dbmodel on table sceneAnalyzers. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. sceneAnalyzerObj: [{JsonCommonSerializer.SerializeToString(sceneAnalyzer)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("4d125b82-d506-4a3d-98ec-acc7c057da5e", $"Error when querying pending queries on table sceneAnalyzers.", ex);
                return null;
            }
        }

        public async Task<bool> DeleteSceneAnalyzerAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var sceneAnalyzer = dbContext.SceneAnalyzers.FirstOrDefault(w => w.ChatId == chatId);

                if (sceneAnalyzer == null)
                {
                    LoggingManager.LogToFile("33bcbe5c-ec7a-4fe0-b1d2-2e7683e43f77", $"SceneAnalyzer tethered to chat [{chatId}] to delete wasn't found in storage.");
                    return false;
                }

                var result = dbContext.SceneAnalyzers.Remove(sceneAnalyzer);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("624b3af4-b7bd-4e6f-9c41-f0ade87c0e1c", $"Error when deleting a specific sceneAnalyzer. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(sceneAnalyzer)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ce5a3c24-d9cc-41e3-afad-60c2b20ceba8", $"Error when querying queries on table SceneAnalyzers.", ex);
                return false;
            }
        }
    }
}
