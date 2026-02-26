using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chat;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/{chatId}")]
    public class ChatController : ControllerBase
    {
        private IChatAddNewMessageWorkflow addNewMessageWorkflow;

        public ChatController(IChatAddNewMessageWorkflow chatAddNewMessageWorkflow)
        {
            addNewMessageWorkflow = chatAddNewMessageWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpPost]
        [Route("addNewMessage")]
        public async Task<IActionResult> AddNewNessage(GetChatByIdRequestDto requestDto)
        {
            return new JsonResult(JsonCommonSerializer.SerializeToString(await addNewMessageWorkflow.AddNewMessageAsync(requestDto)));
        }
    }
}
