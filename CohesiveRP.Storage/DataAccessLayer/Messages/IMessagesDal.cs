using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public interface IMessagesDal
    {
        Task<IMessageDbModel[]> GetHotMessagesAsync(string chatId);
        Task<IMessageDbModel> GetMessageByIdAsync(string chatId, string messageId);
        Task<IMessageDbModel> CreateMessageAsync(CreateMessageQueryModel queryModel);
    }
}
