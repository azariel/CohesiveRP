using CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatCompletionPresetsController : ControllerBase
    {
        private IChatCompletionPresetsWorkflow getChatCompletionPresetsWorkflow;

        public ChatCompletionPresetsController(
            IChatCompletionPresetsWorkflow getChatCompletionPresetsWorkflow)
        {
            this.getChatCompletionPresetsWorkflow = getChatCompletionPresetsWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        public async Task<IActionResult> GetChatCompletionPresets()
        {
            return new JsonResult(await getChatCompletionPresetsWorkflow.GetChatCompletionPresets());
        }
    }
}
