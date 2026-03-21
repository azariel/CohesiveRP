using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Storage.DataAccessLayer.LorebookInstances
{
    public interface ILorebookInstanceDal
    {
        Task<LorebookInstanceDbModel[]> GetLorebookInstancesAsync();
        Task<LorebookInstanceDbModel[]> GetLorebookInstancesAsync(Func<LorebookInstanceDbModel, bool> func);
        Task<LorebookInstanceDbModel> AddLorebookInstanceAsync(LorebookInstanceDbModel dbModel);
        Task<bool> UpdateLorebookInstanceAsync(LorebookInstanceDbModel lorebookDbModel);
        Task<bool> DeleteLorebookInstanceAsync(string chatId);
    }
}
