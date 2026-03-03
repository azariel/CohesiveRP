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
    /// DataAccessLayer around Chats.
    /// </summary>
    public class ChatsDal : StorageDal, IChatsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public ChatsDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<ChatDbModel[]> GetChatsAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                //await dbContext.Chats.LoadAsync();
                return dbContext.Chats.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("0a39887a-ea58-4e78-b44b-afad7e5fc340", $"Error when querying Db on table Chat.", ex);
                return null;
            }
        }

        public async Task<ChatDbModel> GetChatByIdAsync(string id)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                //await dbContext.Chats.LoadAsync();
                return dbContext.Chats.FirstOrDefault(w => w.ChatId == id);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ed1b481f-463b-4854-acac-222965ef3601", $"Error when querying Db on table Chat.", ex);
                return null;
            }
        }

        public async Task<ChatDbModel> CreateChatAsync(CreateChatQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                //dbContext.Chats.Load();

                // Convert models
                ChatDbModel chatDbModel = new ChatDbModel
                {
                    ChatId = Guid.NewGuid().ToString(),
                    InsertDateTimeUtc = DateTime.UtcNow,
                };

                EntityEntry<ChatDbModel> result = await dbContext.Chats.AddAsync(chatDbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("970fa367-1778-45b0-9c75-8243d331f3ea", $"Error when querying Db on table Chat. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("0a39887a-ea58-4e78-b44b-afad7e5fc340", $"Error when querying Db on table Chat.", ex);
                return null;
            }
        }
    }
}
