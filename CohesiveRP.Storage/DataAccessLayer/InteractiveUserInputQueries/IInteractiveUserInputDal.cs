using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries
{
    public interface IInteractiveUserInputDal
    {
        Task<InteractiveUserInputDbModel[]> GetInteractiveUserInputQueriesAsync(Func<InteractiveUserInputDbModel, bool> func);
        Task<InteractiveUserInputDbModel[]> GetInteractiveUserInputQueriesAsync();
        Task<InteractiveUserInputDbModel> AddInteractiveUserInputQueryAsync(InteractiveUserInputDbModel dbModel);
        Task<bool> UpdateInteractiveUserInputQueryAsync(InteractiveUserInputDbModel dbModel);
        Task<bool> DeleteInteractiveUserInputQueryAsync(string interactionUserInputQueryId);
    }
}
