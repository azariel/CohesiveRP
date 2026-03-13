using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions
{
    public interface IChatAddNewMessageWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> AddNewMessageAsync(AddNewMessageRequestDto requestDto);
    }
}
