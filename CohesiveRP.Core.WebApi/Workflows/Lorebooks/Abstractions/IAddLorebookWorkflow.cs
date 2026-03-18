using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions
{
    public interface IAddLorebookWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> AddLorebookAsync(AddNewLorebookRequestDto requestDto);
    }
}
