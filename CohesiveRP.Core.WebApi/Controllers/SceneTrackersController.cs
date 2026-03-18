using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.Workflows.SceneTrackers.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SceneTrackersController : ControllerBase
    {
        private IGetSceneTrackerByIdWorkflow getSceneTrackerByIdWorkflow;
        private IUpdateSceneTrackerWorkflow updateSceneTrackerWorkflow;
        private IForceRefreshSceneTrackerWorkflow forceRefreshSceneTrackerWorkflow;

        public SceneTrackersController(
            IGetSceneTrackerByIdWorkflow getSceneTrackerByIdWorkflow,
            IUpdateSceneTrackerWorkflow updateSceneTrackerWorkflow,
            IForceRefreshSceneTrackerWorkflow forceRefreshSceneTrackerWorkflow)
        {
            this.getSceneTrackerByIdWorkflow = getSceneTrackerByIdWorkflow;
            this.updateSceneTrackerWorkflow = updateSceneTrackerWorkflow;
            this.forceRefreshSceneTrackerWorkflow = forceRefreshSceneTrackerWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        [Route("{chatId}")]
        public async Task<IActionResult> GetSceneTrackerByChatId(string chatId)
        {
            return new JsonResult(await getSceneTrackerByIdWorkflow.GetSceneTrackerByChatId(chatId));
        }

        [HttpPut]
        [Route("{chatId}")]
        public async Task<IActionResult> UpdateSceneTracker(UpdateSceneTrackerRequestDto requestDto)
        {
            return new JsonResult(await updateSceneTrackerWorkflow.UpdateSceneTracker(requestDto));
        }

        [HttpPost]
        [Route("{chatId}")]
        public async Task<IActionResult> ForceRefreshSceneTracker(string chatId)
        {
            return new JsonResult(await forceRefreshSceneTrackerWorkflow.ForceRefreshSceneTracker(chatId));
        }
    }
}
