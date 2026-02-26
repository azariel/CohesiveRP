using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Chat
{
    public interface IChatAddNewMessageWorkflow
    {
        Task<string> AddNewMessageAsync(GetChatByIdRequestDto requestDto);
    }
}
