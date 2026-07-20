using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.InteractiveUserInputQueries;

namespace CohesiveRP.Core.WebApi.Workflows.InteractiveUserInputQueries.Abstractions
{
    public interface IPutInteractiveUserInputQueryWorkflow: IWorkflow
    {
        Task<IWebApiResponseDto> PutInteractiveUserInputQueryAsync(PutInteractiveUserInputQueryRequestDto requestDto);
    }
}
