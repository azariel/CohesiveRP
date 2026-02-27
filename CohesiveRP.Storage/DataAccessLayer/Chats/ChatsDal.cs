using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.Users;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Chats.
    /// </summary>
    public class ChatsDal : StorageDal, IChatsDal, IDisposable
    {
        private StorageDbContext storageDbContext;

        public ChatsDal(JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
        {
            storageDbContext = new StorageDbContext();
            storageDbContext.Chats.Load();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public void Dispose()
        {
            storageDbContext?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<ChatDbModel[]> GetChatsAsync()
        {
            try
            {
                await storageDbContext.Chats.LoadAsync();
                return storageDbContext.Chats.ToArray();
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
                await storageDbContext.Chats.LoadAsync();
                return storageDbContext.Chats.FirstOrDefault(w => w.ChatId == id);
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
                storageDbContext.Chats.Load();

                // Convert models
                ChatDbModel chatDbModel = new ChatDbModel
                {
                    ChatId = Guid.NewGuid().ToString(),
                    InsertDateTimeUtc = DateTime.UtcNow,
                };

                var result = await storageDbContext.Chats.AddAsync(chatDbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("970fa367-1778-45b0-9c75-8243d331f3ea", $"Error when querying Db on table Chat. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await storageDbContext.SaveChangesAsync();
                return chatDbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("0a39887a-ea58-4e78-b44b-afad7e5fc340", $"Error when querying Db on table Chat.", ex);
                return null;
            }
        }
    }
}
