using CohesiveRP.Core.WebApi.Workflows.ChatCharacterRolls.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatCharacterRollsController : ControllerBase
    {
        private IChatCharacterRollsWorkflow getChatCharacterRollsWorkflow;

        public ChatCharacterRollsController(
            IChatCharacterRollsWorkflow getChatCharacterRollsWorkflow)
        {
            this.getChatCharacterRollsWorkflow = getChatCharacterRollsWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        [Route("chats/{chatId}")]
        public async Task<IActionResult> GetChatCharacterRolls(string chatId)
        {
            return new JsonResult(await getChatCharacterRollsWorkflow.GetChatCharacterRolls(chatId));
        }
    }
}
