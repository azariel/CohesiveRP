using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    /// <summary>
    /// DataAccessLayer around Chats.
    /// </summary>
    public class MessagesDal : StorageDal, IMessagesDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public MessagesDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<IMessageDbModel[]> GetHotMessagesAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                //await dbContext.HotMessages.LoadAsync();
                var hotMessages = dbContext.HotMessages.FirstOrDefault(w => w.ChatId == chatId);

                if (hotMessages == null)
                {
                    LoggingManager.LogToFile("450625c7-c63f-481a-9c48-cc9b5d8db509", $"No Hot Messages found for chatId [{chatId}].");
                    return null;
                }

                // Convert the wrapper into individual messages
                return hotMessages.SerializedMessages?.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("54337d85-dc06-436d-88f2-c3d952154a16", $"Error when querying Db on table messages.", ex);
                return null;
            }
        }

        public async Task<IMessageDbModel> GetMessageByIdAsync(string chatId, string messageId)
        {
            try
            {
                var hotMessages = await GetHotMessagesAsync(chatId);
                var message = hotMessages?.FirstOrDefault(w => w.MessageId == messageId);
                if (hotMessages == null || message == null)
                {
                    LoggingManager.LogToFile("365f420cf8-db05-495b-a6b9-a5a9c23f8837", $"Message [{messageId}] in chat [{chatId}] could not be found in HOT messages storage. Parsing COLD storage isn't implemented yet.");
                    return null;// TODO: parse Cold messages here before returning not found
                }

                return message;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("5c5c722c-3cd5-4a22-a528-f6b8b9d8c26b", $"Error when querying Db on table HOT messages.", ex);
                return null;
            }
        }

        public async Task<IMessageDbModel> CreateMessageAsync(CreateMessageQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                //dbContext.HotMessages.Load();

                // Convert model
                MessageDbModel messageDbModel = new MessageDbModel
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Summarized = queryModel.Summarized,
                    Content = queryModel.MessageContent,
                    SourceType = queryModel.SourceType,
                    CreatedAtUtc = queryModel.CreatedAtUtc,
                };

                // Check if HotMessages for this chat already exist
                var chat = dbContext.HotMessages.FirstOrDefault(f => f.ChatId == queryModel.ChatId);

                if (chat == null)
                {
                    // Create the HotMessages row tied to this chat first
                    var newHotMessagesObj = new HotMessagesDbModel
                    {
                        ChatId = queryModel.ChatId,
                        InsertDateTimeUtc = DateTime.UtcNow,
                        SerializedMessages = new List<MessageDbModel> { messageDbModel },
                    };

                    EntityEntry<HotMessagesDbModel> resultAdd = dbContext.HotMessages.Add(newHotMessagesObj);
                    if (resultAdd.State != EntityState.Added)
                    {
                        LoggingManager.LogToFile("b22ec4b0-0707-4521-a932-9af95430a2ae", $"Error when querying Db on table HOT messages. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                        return null;
                    }

                    await dbContext.SaveChangesAsync();
                    return messageDbModel;
                }

                // Otherwise, Update the existing row with the new message
                var currentMessages = chat.SerializedMessages;
                currentMessages.Add(messageDbModel);
                chat.SerializedMessages = currentMessages;
                var resultUpdate = dbContext.HotMessages.Update(chat);
                if (resultUpdate.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("77cb56c7-238d-46e3-a110-d68dc4427a6e", $"Error when querying Db on table HOT messages. State was [{resultUpdate.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultUpdate)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return messageDbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("49529c3a-aa3e-41a9-ad12-209faa0d4047", $"Error when querying Db on table HOT messages.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateHotMessagesAsync(HotMessagesDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Constant fields not to update
                var insertDateTimeUtc = dbContext.HotMessages.AsNoTracking().FirstOrDefault(f=>f.ChatId == dbModel.ChatId)?.InsertDateTimeUtc;

                if(insertDateTimeUtc == null)
                {
                    LoggingManager.LogToFile("fe04e764-2cb2-4f74-9d1e-7d20b801526a", $"Can't update hot messages associated with chatId [{dbModel.ChatId}]. HotMessages related to this chat are not in storage.");
                    return false;
                }

                dbModel.InsertDateTimeUtc = insertDateTimeUtc;

                EntityEntry<HotMessagesDbModel> result = dbContext.HotMessages.Update(dbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("b5bf0a01-1371-4373-be47-b06882018280", $"Error when updating Dbmodel on table messages. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ba7aac69-13d6-402f-8777-b575a455a175", $"Error when querying pending queries on table messages.", ex);
                return false;
            }
        }
    }
}
