using CohesiveRP.Storage.Users;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface IChatsDal
    {
        Task<ChatDbModel> GetChatByIdAsync(string id);
    }
}
