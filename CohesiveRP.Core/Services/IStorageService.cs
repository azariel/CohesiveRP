using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.Services
{
    public interface IStorageService
    {
        Task<ChatDbModel> CreateChatAsync(CreateChatQueryModel queryModel);
        Task<ChatDbModel[]> GetAllChatsAsync();
        Task<ChatDbModel> GetChatAsync(string chatId);
        Task<IMessageDbModel[]> GetAllHotMessages(string chatId);
        Task<IMessageDbModel> GetSpecificMessageAsync(string chatId, string messageId);
        Task<IMessageDbModel> CreateMessageAsync(CreateMessageQueryModel message);
        Task<GlobalSettingsDbModel> GetGlobalSettingsAsync();
        Task<BackgroundQueryDbModel> CreateBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel);
        Task<BackgroundQueryDbModel> GetBackgroundQueryAsync(string queryId);
        Task<ChatCompletionPresetsDbModel> GetChatCompletionPreset(string mainChatCompletionPresetId);
    }
}
