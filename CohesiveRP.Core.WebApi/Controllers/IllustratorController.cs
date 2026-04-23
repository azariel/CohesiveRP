using CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries;
using CohesiveRP.Core.WebApi.Workflows.IllustrationQueries.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IllustratorController : ControllerBase
    {
        private IGetIllustrationQueriesWorkflow getIllustrationQueries;
        private IAddIllustrationQueryWorkflow addIllustrationQuery;

        public IllustratorController(
            IGetIllustrationQueriesWorkflow getIllustrationQueries,
            IAddIllustrationQueryWorkflow addIllustrationQuery)
        {
            this.getIllustrationQueries = getIllustrationQueries;
            this.addIllustrationQuery = addIllustrationQuery;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        // Queries
        [HttpGet]
        [Route("queries")]
        public async Task<IActionResult> GetIllustrationQueries([FromRoute] string chatId)
        {
            return new JsonResult(await getIllustrationQueries.GetQueries(chatId));
        }

        [HttpPost]
        [Route("queries")]
        public async Task<IActionResult> AddIllustrationQueries(AddIllustrationQueryRequestDto requestDto)
        {
            return new JsonResult(await addIllustrationQuery.AddQuery(requestDto));
        }
    }
}
