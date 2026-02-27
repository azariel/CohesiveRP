using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions
{
    public interface IChatAddNewMessageWorkflow
    {
        Task<IWebApiResponseDto> AddNewMessageAsync(AddNewMessageRequestDto requestDto);
    }
}
