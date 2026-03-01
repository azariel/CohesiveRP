using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;
using CohesiveRP.Storage.Users;

namespace CohesiveRP.Core.Services
{
    public interface IStorageService
    {
        Task<ChatDbModel> CreateChatAsync(CreateChatQueryModel queryModel);
        Task<ChatDbModel[]> GetAllChatsAsync();
        Task<ChatDbModel> GetChatAsync(string chatId);
        Task<IMessageDbModel[]> GetAllHotMessages(string chatId);
        Task<IMessageDbModel> CreateMessageAsync(CreateMessageQueryModel message);
        Task<GlobalSettingsDbModel> GetGlobalSettingsAsync();
    }
}
