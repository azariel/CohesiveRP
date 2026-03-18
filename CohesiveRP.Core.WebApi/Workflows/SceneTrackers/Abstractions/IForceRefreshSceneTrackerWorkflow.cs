using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.SceneTrackers.Abstractions
{
    public interface IForceRefreshSceneTrackerWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> ForceRefreshSceneTracker(string chatId);
    }
}
