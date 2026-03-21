using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.Lorebooks;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface ILorebooksDal
    {
        Task<LorebookDbModel[]> GetLorebooksAsync();
        Task<LorebookDbModel[]> GetLorebooksByFuncAsync(Func<LorebookDbModel, bool> func);
        Task<LorebookDbModel> GetLorebookByIdAsync(string lorebookId);
        Task<LorebookDbModel> AddLorebookAsync(AddLorebookQueryModel queryModel);
        Task<LorebookDbModel> AddLorebookAsync(LorebookDbModel dbModel);
        Task<bool> UpdateLorebookAsync(LorebookDbModel lorebookDbModel);
        Task<bool> DeleteLorebookAsync(LorebookDbModel lorebookDbModel);
    }
}
