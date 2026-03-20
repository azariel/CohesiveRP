using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface IChatsDal
    {
        Task<ChatDbModel[]> GetChatsAsync();
        Task<ChatDbModel> GetChatByIdAsync(string id);
        Task<ChatDbModel> CreateChatAsync(CreateChatQueryModel queryModel);
        Task<bool> DeleteChatAsync(string chatId);
    }
}
