using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackgroundQueriesController : ControllerBase
    {
        private IGetBackgroundQueryWorkflow getBackgroundQueryWorkflow;

        public BackgroundQueriesController(
            IGetBackgroundQueryWorkflow getBackgroundQueryWorkflow)
        {
            this.getBackgroundQueryWorkflow = getBackgroundQueryWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [Route("{queryId}")]
        [HttpGet]
        public async Task<IActionResult> GetBackgroundQuery(string queryId)
        {
            return new JsonResult(await getBackgroundQueryWorkflow.GetBackgroundQuery(queryId));
        }
    }
}
