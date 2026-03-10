using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public interface IMessagesDal
    {
        Task<IMessageDbModel[]> GetHotMessagesAsync(string chatId);
        Task<IMessageDbModel> GetMessageByIdAsync(string chatId, string messageId);
        Task<IMessageDbModel> CreateOrUpdateMessageAsync(CreateMessageQueryModel queryModel);
        Task<bool> UpdateHotMessagesAsync(HotMessagesDbModel messages);
        Task<bool> UpdateHotMessageAsync(string chatId, MessageDbModel message);
    }
}
