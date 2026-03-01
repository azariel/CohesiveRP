using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;
using CohesiveRP.Storage.Users;

namespace CohesiveRP.Core.Services
{
    /// <summary>
    /// Expose and handle operations to the Storage service.
    /// </summary>
    public class StorageService : IStorageService
    {
        private IChatsDal chatsDal;
        private IMessagesDal messagesDal;
        private IGlobalSettingsDal globalSettingsDal;

        public StorageService(IChatsDal chatsDal, IMessagesDal messagesDal, IGlobalSettingsDal globalSettingsDal)
        {
            this.chatsDal = chatsDal;
            this.messagesDal = messagesDal;
            this.globalSettingsDal = globalSettingsDal;
        }

        // Chats
        public async Task<ChatDbModel> CreateChatAsync(CreateChatQueryModel queryModel)
        {
            return await chatsDal.CreateChatAsync(queryModel);
        }

        public async Task<ChatDbModel[]> GetAllChatsAsync()
        {
            return await chatsDal.GetChatsAsync();
        }

        public async Task<ChatDbModel> GetChatAsync(string chatId)
        {
            return await chatsDal.GetChatByIdAsync(chatId);
        }

        // Messages

        public async Task<IMessageDbModel[]> GetAllHotMessages(string chatId)
        {
            return await messagesDal.GetHotMessagesAsync(chatId);
        }

        public async Task<IMessageDbModel> CreateMessageAsync(CreateMessageQueryModel message)
        {
            return await messagesDal.CreateMessageAsync(message);
        }

        // Settings
        public async Task<GlobalSettingsDbModel> GetGlobalSettingsAsync()
        {
            return await globalSettingsDal.GetGlobalSettingsAsync();
        }
    }
}
