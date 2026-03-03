using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.Users;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface IChatsDal
    {
        Task<ChatDbModel[]> GetChatsAsync();
        Task<ChatDbModel> GetChatByIdAsync(string id);
        Task<ChatDbModel> CreateChatAsync(CreateChatQueryModel queryModel);
    }
}
