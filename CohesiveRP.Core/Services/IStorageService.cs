using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;
using CohesiveRP.Storage.Users;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;

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
        Task<BackgroundQueryDbModel> CreateBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel);
    }
}
