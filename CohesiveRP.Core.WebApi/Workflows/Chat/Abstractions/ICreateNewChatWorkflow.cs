using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions
{
    public interface ICreateNewChatWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> AddNewChatAsync(AddNewChatRequestDto requestDto);
    }
}
