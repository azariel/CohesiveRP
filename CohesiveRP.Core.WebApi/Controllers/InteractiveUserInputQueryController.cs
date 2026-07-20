using CohesiveRP.Core.WebApi.RequestDtos.InteractiveUserInputQueries;
using CohesiveRP.Core.WebApi.Workflows.InteractiveUserInputQueries.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InteractiveUserInputQueryController : ControllerBase
    {
        private IGetInteractiveUserInputQueriesFromChatIdWorkflow getInteractiveUserInputQueriesFromChatIdWorkflow;
        private IPutInteractiveUserInputQueryWorkflow putInteractiveUserInputQueryWorkflow;

        public InteractiveUserInputQueryController(
            IGetInteractiveUserInputQueriesFromChatIdWorkflow getInteractiveUserInputQueriesFromChatIdWorkflow,
            IPutInteractiveUserInputQueryWorkflow putInteractiveUserInputQueryWorkflow)
        {
            this.getInteractiveUserInputQueriesFromChatIdWorkflow = getInteractiveUserInputQueriesFromChatIdWorkflow;
            this.putInteractiveUserInputQueryWorkflow = putInteractiveUserInputQueryWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [Route("chats/{chatId}")]
        [HttpGet]
        public async Task<IActionResult> GetBackgroundQueryByChatId([FromRoute] string chatId)
        {
            return new JsonResult(await getInteractiveUserInputQueriesFromChatIdWorkflow.GetInteractiveUserInputQueryAsync(chatId));
        }

        [Route("{queryId}")]
        [HttpPut]
        public async Task<IActionResult> PutBackgroundQuery(PutInteractiveUserInputQueryRequestDto requestDto)
        {
            return new JsonResult(await putInteractiveUserInputQueryWorkflow.PutInteractiveUserInputQueryAsync(requestDto));
        }
    }
}
