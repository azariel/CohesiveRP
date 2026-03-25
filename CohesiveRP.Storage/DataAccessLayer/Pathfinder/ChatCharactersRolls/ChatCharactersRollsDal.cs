using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around ChatCharactersRolls.
    /// </summary>
    public class ChatCharactersRollsDal : StorageDal, IChatCharactersRollsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public ChatCharactersRollsDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<ChatCharactersRollsDbModel[]> GetChatCharactersRollsAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.ChatCharactersRolls.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("4da39846-6eea-43f0-a5a6-446326612346", $"Error when querying Db on table ChatCharactersRolls.", ex);
                return null;
            }
        }

        public async Task<ChatCharactersRollsDbModel[]> GetChatCharactersRollsByFuncAsync(Func<ChatCharactersRollsDbModel, bool> func)
        {
            if (func == null)
            {
                return null;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var result = await dbContext.ChatCharactersRolls.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("b36c6cab-5b78-4d6a-83a5-3778332ad696", $"Error when querying Db on table ChatCharactersRolls.", ex);
                return null;
            }
        }

        public async Task<ChatCharactersRollsDbModel> GetChatCharactersRollsEntryAsync(string chatId)
        {
            var characters = await GetChatCharactersRollsByFuncAsync(f => f.ChatId == chatId);
            return characters?.FirstOrDefault();
        }

        public async Task<ChatCharactersRollsDbModel> AddChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Override system fields
                dbModel.CreatedAtUtc = DateTime.UtcNow;
                dbModel.LastActivityAtUtc = DateTime.UtcNow;

                var result = await dbContext.ChatCharactersRolls.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("d4ac3a62-df76-4ee2-98be-af8dcfe78e72", $"Error when querying Db on table ChatCharactersRolls. State was [{result.State}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("47ecade5-9ea3-4ffa-8914-16f90c91f8fc", $"Error when querying Db on table ChatCharactersRolls.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var rolls = dbContext.ChatCharactersRolls.FirstOrDefault(w => w.ChatId == dbModel.ChatId);

                if (rolls == null)
                {
                    LoggingManager.LogToFile("11517054-bac4-4d69-b412-2aec77df7c63", $"ChatCharactersRoll entry tethered to chatid [{dbModel.ChatId}] to update wasn't found in storage.");
                    return false;
                }

                // System fields
                rolls.LastActivityAtUtc = DateTime.UtcNow;

                // Update only the overridable fields
                rolls.ChatCharactersRolls = dbModel.ChatCharactersRolls;

                var result = dbContext.ChatCharactersRolls.Update(rolls);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("d24636f6-59eb-4f7e-b0b2-42b7b14f1caa", $"Error when updating a ChatCharactersRoll obj. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("44649f37-6a8a-48fc-abd1-b011e386ec6d", $"Error when querying pending queries on table ChatCharactersRolls.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var rolls = dbContext.ChatCharactersRolls.AsNoTracking().FirstOrDefault(w => w.ChatId == dbModel.ChatId);

                if (rolls == null)
                {
                    LoggingManager.LogToFile("c59f228a-b72a-47ee-b4b2-da4f8e2b4fc7", $"CharacterSheet tethered to characterId [{dbModel.ChatId}] to delete wasn't found in storage.");
                    return false;
                }

                var result = dbContext.ChatCharactersRolls.Remove(dbModel);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("4d5bc7de-a48b-4aa2-9dd7-aeb2be6396b7", $"Error when deleting a specific ChatCharactersRoll obj. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("bab1f7bd-d2d2-482b-b091-46954244a4c6", $"Error when querying pending queries on table ChatCharactersRolls.", ex);
                return false;
            }
        }
    }
}
