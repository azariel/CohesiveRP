using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Storage.WebApi.Workflows.Chats
{
    public interface IGetChatWorkflow
    {
        Task<IWebApiReponseDto> GetChatByIdAsync(GetChatByIdRequestDto getChatByIdRequestDto);
    }
}
