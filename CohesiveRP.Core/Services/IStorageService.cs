using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.Services
{
    public interface IStorageService
    {
        Task<IWebApiReponseDto> GetChatAsync(string chatId);
    }
}
