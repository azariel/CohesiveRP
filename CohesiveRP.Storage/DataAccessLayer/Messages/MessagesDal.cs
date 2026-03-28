using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
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

        /// <summary>
        /// Push the oldest Hot message into the Cold storage if the amount of hot messages is too great to keep hot messages model lean and fast.
        /// </summary>
        private async Task HandleColdStorageAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                HotMessagesDbModel hotMessages = dbContext.HotMessages.FirstOrDefault(w => w.ChatId == chatId);
                if (hotMessages == null)
                {
                    LoggingManager.LogToFile("dc27c98b-690d-4bac-baeb-589759f5ae1e", $"No Hot Messages found for chatId [{chatId}] when handling Cold storage logic.");
                    return;
                }

                GlobalSettingsDbModel settings = dbContext.GlobalSettings.FirstOrDefault();
                int? nbMaxHotMessages = settings?.Summary?.HotMessagesAmountLimit;
                if (nbMaxHotMessages == null)
                {
                    LoggingManager.LogToFile("aee83319-e96a-4e6f-9218-6d3267e999fe", $"No GlobalSettings found for chatId [{chatId}] when handling Cold storage logic.");
                    return;
                }

                if (hotMessages.Messages.Count < nbMaxHotMessages)
                {
                    return;
                }

                // Take the X oldest hot messages and put them into cold storage (we're doing three to avoid having to do this every single time
                int nbMessagesToTransfert = settings.Summary.HotMessagesAmountLimit / 5;
                MessageDbModel[] messagesToTransfert = hotMessages.Messages.OrderBy(o => o.CreatedAtUtc).Take(nbMessagesToTransfert).ToArray();

                // Add those messages to Cold Storage
                int? nbColdMessages = await AddMessagesToColdStorageAsync(dbContext, settings, messagesToTransfert, chatId, nbMessagesToTransfert);

                // Remove those messages from HOT storage
                var currentMessages = hotMessages.Messages;

                foreach (var messageToRemove in messagesToTransfert)
                {
                    currentMessages.Remove(messageToRemove);
                }

                if (nbColdMessages != null && nbColdMessages.HasValue)
                {
                    hotMessages.NbColdMessages = nbColdMessages.Value;
                }

                hotMessages.Messages = currentMessages;
                var resultUpdate = dbContext.HotMessages.Update(hotMessages);
                if (resultUpdate.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("241d22ae-09ca-4913-aba1-26353f6cd32e", $"Error when querying Db on table HOT messages. Couldn't remove old messages past the configured HOT limitation. State was [{resultUpdate.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultUpdate)}].");
                    return;
                }

                dbContext.SaveChanges();

            } catch (Exception ex)
            {
                LoggingManager.LogToFile("c018d21d-38d7-4576-9d2a-83624687aaaa", $"Error when querying Db on table messages.", ex);
            }
        }

        private async Task<int?> AddMessagesToColdStorageAsync(StorageDbContext dbContext, GlobalSettingsDbModel globalSettings, MessageDbModel[] messagesToTransfert, string chatId, int nbMessagesToTransfert)
        {
            // Check if ColdMessages for this chat already exist
            var coldMessages = dbContext.ColdMessages.FirstOrDefault(f => f.ChatId == chatId);
            if (coldMessages == null)
            {
                // Create the ColdMessages row tied to this chat first
                var newColdMessagesObj = new ColdMessagesDbModel
                {
                    ChatId = chatId,
                    CreatedAtUtc = DateTime.UtcNow,
                    Messages = new List<MessageDbModel>(),
                };

                EntityEntry<ColdMessagesDbModel> resultAdd = dbContext.ColdMessages.Add(newColdMessagesObj);
                if (resultAdd.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("9d7af073-4e21-4f4d-85a7-56dfc5287109", $"Error when querying Db on table COLD messages. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                coldMessages = dbContext.ColdMessages.FirstOrDefault(f => f.ChatId == chatId);
            }

            // Add the new messages
            var currentMessages = coldMessages.Messages;
            currentMessages.AddRange(messagesToTransfert);
            coldMessages.Messages = currentMessages;

            var resultUpdate = dbContext.ColdMessages.Update(coldMessages);
            if (resultUpdate.State != EntityState.Modified)
            {
                LoggingManager.LogToFile("c72298d3-4d84-4a5d-9641-dabb68cdcc83", $"Error when querying Db on table COLD messages. State was [{resultUpdate.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultUpdate)}].");
                return null;
            }

            // Do some cleanup. We want to keep at most X cold messages
            if (coldMessages.Messages.Count < globalSettings.Summary.ColdMessagesAmountLimit)
            {
                return coldMessages.Messages.Count;
            }

            List<MessageDbModel> messagesToRemove = [.. coldMessages.Messages.OrderBy(o => o.CreatedAtUtc).Take(nbMessagesToTransfert)];

            currentMessages = coldMessages.Messages;

            foreach (var messageToRemove in messagesToRemove)
            {
                currentMessages.Remove(messageToRemove);
            }

            coldMessages.Messages = currentMessages;
            resultUpdate = dbContext.ColdMessages.Update(coldMessages);

            if (resultUpdate.State != EntityState.Modified)
            {
                LoggingManager.LogToFile("d4cceec5-3c3e-4ccf-9848-84a384367fa9", $"Error when querying Db on table COLD messages. Couldn't remove old messages past the configured COLD limitation. State was [{resultUpdate.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultUpdate)}].");
                return null;
            }

            dbContext.SaveChanges();
            return coldMessages.Messages.Count;
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<HotMessagesDbModel> GetHotMessagesAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var hotMessages = dbContext.HotMessages.FirstOrDefault(w => w.ChatId == chatId);

                if (hotMessages == null)
                {
                    LoggingManager.LogToFile("450625c7-c63f-481a-9c48-cc9b5d8db509", $"No Hot Messages found for chatId [{chatId}].");
                    return null;
                }

                // Convert the wrapper into individual messages
                return hotMessages;
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
                var message = hotMessages?.Messages?.FirstOrDefault(w => w.MessageId == messageId);
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

        public async Task<IMessageDbModel> CreateOrUpdateMessageAsync(CreateMessageQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Convert model
                MessageDbModel messageDbModel = new MessageDbModel
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Summarized = queryModel.Summarized,
                    Content = queryModel.MessageContent,
                    SourceType = queryModel.SourceType,
                    CreatedAtUtc = queryModel.CreatedAtUtc,
                    CharacterId = queryModel.CharacterId,
                    AvatarFilePath = queryModel.AvatarFilePath,
                };

                // Check if HotMessages for this chat already exist
                var hotMessages = dbContext.HotMessages.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (hotMessages == null)
                {
                    // Create the HotMessages row tied to this chat first
                    var newHotMessagesObj = new HotMessagesDbModel
                    {
                        ChatId = queryModel.ChatId,
                        CreatedAtUtc = DateTime.UtcNow,
                        NbColdMessages = 0,
                        Messages = new List<MessageDbModel> { messageDbModel },
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
                var currentMessages = hotMessages.Messages;
                currentMessages.Add(messageDbModel);
                hotMessages.Messages = currentMessages;
                var resultUpdate = dbContext.HotMessages.Update(hotMessages);
                if (resultUpdate.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("77cb56c7-238d-46e3-a110-d68dc4427a6e", $"Error when querying Db on table HOT messages. State was [{resultUpdate.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultUpdate)}].");
                    return null;
                }

                // Update linked chat
                var chat = dbContext.Chats.FirstOrDefault(f => f.ChatId == queryModel.ChatId);
                if (hotMessages != null)
                {
                    chat.LastActivityAtUtc = DateTime.UtcNow;
                    dbContext.Chats.Update(chat);
                }

                await dbContext.SaveChangesAsync();

                // Handle cold storage in an async process
                _ = HandleColdStorageAsync(queryModel.ChatId);

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
                var initialDbModel = dbContext.HotMessages.AsNoTracking().FirstOrDefault(f => f.ChatId == dbModel.ChatId);
                if (initialDbModel == null)
                {
                    LoggingManager.LogToFile("fe04e764-2cb2-4f74-9d1e-7d20b801526a", $"Can't update hot messages associated with chatId [{dbModel.ChatId}]. HotMessages related to this chat are not in storage.");
                    return false;
                }

                initialDbModel.Messages = dbModel.Messages;

                EntityEntry<HotMessagesDbModel> result = dbContext.HotMessages.Update(initialDbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("b5bf0a01-1371-4373-be47-b06882018280", $"Error when updating Dbmodel on table messages. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                // Update linked chat
                var chat = dbContext.Chats.FirstOrDefault(f => f.ChatId == dbModel.ChatId);
                if (chat != null)
                {
                    chat.LastActivityAtUtc = DateTime.UtcNow;
                    dbContext.Chats.Update(chat);
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ba7aac69-13d6-402f-8777-b575a455a175", $"Error when querying pending queries on table messages.", ex);
                return false;
            }
        }

        public async Task<bool> UpdateHotMessageAsync(string chatId, MessageDbModel message)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Constant fields not to update
                var hotMessages = dbContext.HotMessages.FirstOrDefault(f => f.ChatId == chatId);
                if (hotMessages?.Messages == null)
                {
                    LoggingManager.LogToFile("f747bce8-4559-4bcd-bcf4-9387e78079fd", $"Can't update hot message associated with chatId [{chatId}]. HotMessages related to this chat are not in storage or messages to update didn't exist.");
                    return false;
                }

                var index = hotMessages.Messages.FindIndex(f => f.MessageId == message.MessageId);
                if (index >= 0)
                {
                    hotMessages.Messages[index] = message;
                } else
                {
                    LoggingManager.LogToFile("8b0d7de3-5a7c-4c4f-97b6-af80742eb556", $"Message [{message.MessageId}] not found in hot messages for chat [{chatId}].");
                    return false;
                }

                EntityEntry<HotMessagesDbModel> result = dbContext.HotMessages.Update(hotMessages);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("afc42bed-13f5-49b7-8c43-a317225ca1cc", $"Error when updating Dbmodel on table messages. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. message: [{JsonCommonSerializer.SerializeToString(message)}].");
                    return false;
                }

                // Update linked chat
                var chat = dbContext.Chats.FirstOrDefault(f => f.ChatId == chatId);
                if (chat != null)
                {
                    chat.LastActivityAtUtc = DateTime.UtcNow;
                    dbContext.Chats.Update(chat);
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("d51ab3f9-63e8-41fe-a4c9-0da0855d78de", $"Error when querying pending queries on table messages.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteSpecificMessageAsync(string chatId, string messageId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                HotMessagesDbModel chatHotMessages = dbContext.HotMessages.FirstOrDefault(w => w.ChatId == chatId);
                var message = chatHotMessages?.Messages?.FirstOrDefault(w => w.MessageId == messageId);

                if (chatHotMessages == null || message == null)
                {
                    LoggingManager.LogToFile("08ccbeee-a4a3-42a2-ba7e-8d814192773e", $"Message [{messageId}] in chat [{chatId}] could not be found in HOT messages storage. Parsing COLD storage isn't implemented yet. Can't delete this message.");
                    return false;// TODO: parse Cold messages here before returning not found
                }

                chatHotMessages.Messages.Remove(message);
                EntityEntry<HotMessagesDbModel> result = dbContext.HotMessages.Update(chatHotMessages);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("c0c4f9f1-9b41-4942-ac0a-7cf3984c1de2", $"Error when deleting Dbmodel on table messages. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. message: [{JsonCommonSerializer.SerializeToString(message)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ae78540c-8b82-4973-9439-c26dff6830cb", $"Error when querying Db on table HOT messages to delete message.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteColdMessageAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                HotMessagesDbModel messagesObj = dbContext.HotMessages.FirstOrDefault(w => w.ChatId == chatId);

                if (messagesObj == null)
                {
                    LoggingManager.LogToFile("902f1973-b0c8-43a3-a3ec-debbcf17ba86", $"Cold Messages tied to Chat [{chatId}] to delete weren't found in storage.");
                    return false;
                }

                var result = dbContext.HotMessages.Remove(messagesObj);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("36701052-c788-4fa6-acf4-9f237c8de4c7", $"Error when deleting a specific Cold message object. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(messagesObj)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("1e53acbf-ea6b-4006-84a4-346182bfe93d", $"Error when querying pending queries on table Cold messages.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteHotMessageAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                HotMessagesDbModel messagesObj = dbContext.HotMessages.FirstOrDefault(w => w.ChatId == chatId);

                if (messagesObj == null)
                {
                    LoggingManager.LogToFile("9df41399-7c81-4b11-bb33-5ee50da5b5eb", $"Hot Messages tied to Chat [{chatId}] to delete weren't found in storage.");
                    return false;
                }

                var result = dbContext.HotMessages.Remove(messagesObj);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("4d033e9d-9fe1-4f2b-947c-cbf49e8dcca6", $"Error when deleting a specific Hot message object. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(messagesObj)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("8def56ce-ba0a-4800-8992-aaa8848a6073", $"Error when querying pending queries on table Hot messages.", ex);
                return false;
            }
        }
    }
}
