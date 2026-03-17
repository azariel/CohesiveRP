using CohesiveRP.Core.WebApi.Workflows.SceneTrackers.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SceneTrackersController : ControllerBase
    {
        private IGetSceneTrackerByIdWorkflow getSceneTrackerByIdWorkflow;

        public SceneTrackersController(
            IGetSceneTrackerByIdWorkflow getSceneTrackerByIdWorkflow)
        {
            this.getSceneTrackerByIdWorkflow = getSceneTrackerByIdWorkflow;
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
    }
}
