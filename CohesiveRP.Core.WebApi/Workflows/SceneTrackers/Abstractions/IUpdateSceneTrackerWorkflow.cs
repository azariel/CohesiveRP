using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;

namespace CohesiveRP.Core.WebApi.Workflows.SceneTrackers.Abstractions
{
    public interface IUpdateSceneTrackerWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> UpdateSceneTracker(UpdateSceneTrackerRequestDto requestDto);
    }
}
