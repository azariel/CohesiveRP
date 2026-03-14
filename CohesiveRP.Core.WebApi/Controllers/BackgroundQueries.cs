using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackgroundQueriesController : ControllerBase
    {
        private IGetBackgroundQueryWorkflow getBackgroundQueryWorkflow;
        private IGetBackgroundQueriesByChatIdWorkflow getBackgroundQueriesByChatIdWorkflow;

        public BackgroundQueriesController(
            IGetBackgroundQueryWorkflow getBackgroundQueryWorkflow,
            IGetBackgroundQueriesByChatIdWorkflow getBackgroundQueriesByChatIdWorkflow)
        {
            this.getBackgroundQueryWorkflow = getBackgroundQueryWorkflow;
            this.getBackgroundQueriesByChatIdWorkflow = getBackgroundQueriesByChatIdWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [Route("{queryId}")]
        [HttpGet]
        public async Task<IActionResult> GetBackgroundQueryByQueryId(string queryId)
        {
            return new JsonResult(await getBackgroundQueryWorkflow.GetBackgroundQuery(queryId));
        }

        [HttpGet]
        public async Task<IActionResult> GetBackgroundQueryByChatId([FromQuery] string chatId)
        {
            return new JsonResult(await getBackgroundQueriesByChatIdWorkflow.GetBackgroundQueries(chatId));
        }
    }
}
