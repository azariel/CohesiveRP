using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface IChatCharactersRollsDal
    {
        Task<ChatCharactersRollsDbModel[]> GetChatCharactersRollsAsync();
        Task<ChatCharactersRollsDbModel> GetChatCharactersRollsEntryAsync(string chatId);
        Task<ChatCharactersRollsDbModel[]> GetChatCharactersRollsByFuncAsync(Func<ChatCharactersRollsDbModel, bool> func);
        Task<ChatCharactersRollsDbModel> AddChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel);
        Task<bool> UpdateChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel);
        Task<bool> DeleteChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel);
    }
}
