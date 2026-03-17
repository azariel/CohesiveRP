using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.SceneTrackers.Abstractions
{
    public interface IGetSceneTrackerByIdWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetSceneTrackerByChatId(string chatId);
    }
}
