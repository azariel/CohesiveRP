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

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<ChatDbModel[]> GetChatsAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
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

                // Convert models
                ChatDbModel chatDbModel = new ChatDbModel
                {
                    ChatId = Guid.NewGuid().ToString(),
                    Name = queryModel.Name,
                    CreatedAtUtc = DateTime.UtcNow,
                    SelectedChatCompletionPresets = queryModel.SelectedChatCompletionPresets,
                    CharacterIds = queryModel.CharacterIds,
                    LorebookIds = queryModel.LorebookIds,
                    PersonaId = queryModel.PersonaId,
                    LastActivityAtUtc = DateTime.UtcNow,
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

        public async Task<bool> UpdateChatAsync(ChatDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var chat = dbContext.Chats.FirstOrDefault(w => w.ChatId == dbModel.ChatId);

                if (chat == null)
                {
                    LoggingManager.LogToFile("e89002b8-9b09-4ddc-b1fd-53b5cea3327f", $"Chat [{dbModel.ChatId}] to update wasn't found in storage.");
                    return false;
                }

                // Update only the overridable fields
                chat.SelectedChatCompletionPresets = dbModel.SelectedChatCompletionPresets;
                chat.LorebookIds = dbModel.LorebookIds;
                chat.CharacterIds = dbModel.CharacterIds;
                chat.Name = dbModel.Name;
                chat.PersonaId = dbModel.PersonaId;

                var result = dbContext.Chats.Update(chat);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("5c3ae68d-9c28-44b0-9f50-d816e735ff78", $"Error when updating a Chat. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("eeecbf0f-eb8d-4191-848c-5d1de62f9fa8", $"Error when querying pending queries on table Chats.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteChatAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var chat = dbContext.Chats.FirstOrDefault(w => w.ChatId == chatId);

                if (chat == null)
                {
                    LoggingManager.LogToFile("f23deb92-034e-43dc-a7c4-4e0ca1c8c5a5", $"Chat [{chatId}] to delete wasn't found in storage.");
                    return false;
                }

                var result = dbContext.Chats.Remove(chat);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("cbc84e32-b1ed-4c3d-8a4a-03810a0afddc", $"Error when deleting a specific Chat. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(chat)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("8173a545-f6d2-43c8-8caf-6b1bcf5e497c", $"Error when querying pending queries on table Chats.", ex);
                return false;
            }
        }
    }
}
