using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;
using CohesiveRP.Storage.Users;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;

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
        private IBackgroundQueriesDal backgroundQueriesDal;

        public StorageService(IChatsDal chatsDal, IMessagesDal messagesDal, IGlobalSettingsDal globalSettingsDal, IBackgroundQueriesDal backgroundQueriesDal)
        {
            this.chatsDal = chatsDal;
            this.messagesDal = messagesDal;
            this.globalSettingsDal = globalSettingsDal;
            this.backgroundQueriesDal = backgroundQueriesDal;
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

        // BackgroundQueries
        public async Task<BackgroundQueryDbModel> CreateBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel)
        {
            return await backgroundQueriesDal.CreateBackgroundQueryAsync(queryModel);
        }
    }
}
