using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;

namespace CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions
{
    public interface IUpdateLorebookWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> UpdateLorebookAsync(UpdateLorebookRequestDto requestDto);
    }
}
