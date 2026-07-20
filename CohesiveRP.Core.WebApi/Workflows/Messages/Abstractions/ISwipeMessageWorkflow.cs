using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions
{
    public interface ISwipeMessageWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> SwipeMessageAsync(GetSpecificMessageRequestDto requestDto);
    }
}
