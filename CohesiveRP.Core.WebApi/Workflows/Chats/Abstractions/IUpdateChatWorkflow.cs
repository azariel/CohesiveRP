using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions
{
    public interface IUpdateChatWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> UpdateChatAsync(UpdateChatRequestDto requestDto);
    }
}
