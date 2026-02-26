using CohesiveRP.Core.WebApi.ResponseDtos.Storage;

namespace CohesiveRP.Core.WebApi.Services
{
    public interface IStorageService
    {
        Task<ChatResponseDto> GetChatAsync(string chatId);
    }
}
