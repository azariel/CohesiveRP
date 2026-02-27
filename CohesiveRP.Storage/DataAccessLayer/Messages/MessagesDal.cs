using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.Message;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    /// <summary>
    /// DataAccessLayer around Chats.
    /// </summary>
    public class MessagesDal : StorageDal, IMessagesDal, IDisposable
    {
        private StorageDbContext storageDbContext;

        public MessagesDal(JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
        {
            storageDbContext = new StorageDbContext();
            storageDbContext.HotMessages.Load();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public void Dispose()
        {
            storageDbContext?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<IMessageDbModel[]> GetHotMessagesAsync(string chatId)
        {
            try
            {
                await storageDbContext.HotMessages.LoadAsync();
                var hotMessages = storageDbContext.HotMessages.FirstOrDefault(w => w.ChatId == chatId);

                if (hotMessages == null)
                {
                    LoggingManager.LogToFile("450625c7-c63f-481a-9c48-cc9b5d8db509", $"No Hot Messages found for chatId [{chatId}].");
                    return null;
                }

                // Convert the wrapper into individual messages
                List<IMessageDbModel> messages = [];
                var models = JsonCommonSerializer.DeserializeFromString<MessageDbModel[]>(hotMessages.SerializedMessages);
                return models;
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
                storageDbContext.HotMessages.Load();

                // Convert model
                MessageDbModel messageDbModel = new MessageDbModel
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Content = queryModel.messageContent,
                };

                // Check if HotMessages for this chat already exist
                var chat = storageDbContext.HotMessages.FirstOrDefault(f => f.ChatId == queryModel.chatId);

                if (chat == null)
                {
                    // Create the HotMessages row tied to this chat first
                    var newHotMessagesObj = new HotMessagesDbModel
                    {
                        ChatId = queryModel.chatId,
                        InsertDateTimeUtc = DateTime.UtcNow,
                        SerializedMessages = JsonCommonSerializer.SerializeToString(new[] { messageDbModel }),
                    };

                    var resultAdd = storageDbContext.HotMessages.Add(newHotMessagesObj);
                    if (resultAdd.State != EntityState.Added)
                    {
                        LoggingManager.LogToFile("b22ec4b0-0707-4521-a932-9af95430a2ae", $"Error when querying Db on table HOT messages. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                        return null;
                    }

                    await storageDbContext.SaveChangesAsync();
                    return messageDbModel;
                }

                // Otherwise, Update the existing row with the new message
                var currentMessages = JsonCommonSerializer.DeserializeFromString<IMessageDbModel[]>(chat.SerializedMessages)?.ToList();
                currentMessages.Add(messageDbModel);
                chat.SerializedMessages = JsonCommonSerializer.SerializeToString(currentMessages);
                var resultUpdate = storageDbContext.HotMessages.Update(chat);
                if (resultUpdate.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("77cb56c7-238d-46e3-a110-d68dc4427a6e", $"Error when querying Db on table HOT messages. State was [{resultUpdate.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultUpdate)}].");
                    return null;
                }

                await storageDbContext.SaveChangesAsync();
                return messageDbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("49529c3a-aa3e-41a9-ad12-209faa0d4047", $"Error when querying Db on table HOT messages.", ex);
                return null;
            }
        }
    }
}
