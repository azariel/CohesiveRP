using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.WebApi.RequestDtos.Chat;
using CohesiveRP.Storage.WebApi.Workflows.Chats;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private IGetChatWorkflow getChatWorkflow;

        public ChatsController(IGetChatWorkflow getChatWorkflow)
        {
            this.getChatWorkflow = getChatWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        [Route("{chatId}")]
        public async Task<IActionResult> GetChat(GetChatByIdRequestDto requestDto)
        {
            var result = await getChatWorkflow.GetChatByIdAsync(requestDto);
            return new JsonResult(result);
        }
    }
}
