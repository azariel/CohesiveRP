using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Chat
{
    public interface IChatAddNewMessageWorkflow
    {
        Task<IWebApiReponseDto> AddNewMessageAsync(GetChatByIdRequestDto requestDto);
    }
}
