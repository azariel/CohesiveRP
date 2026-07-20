using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries;

namespace CohesiveRP.Core.WebApi.Workflows.IllustrationQueries.Abstractions
{
    public interface IAddIllustrationQueryWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> AddQuery(AddIllustrationQueryRequestDto requestDto);
    }
}
