using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/{chatId}")]
    public class ChatController : ControllerBase
    {
        private IGetAllHotMessagesWorkflow getAllHotMessagesWorkflow;
        private IChatAddNewMessageWorkflow addNewMessageWorkflow;

        public ChatController(
            IGetAllHotMessagesWorkflow getAllHotMessagesWorkflow,
            IChatAddNewMessageWorkflow chatAddNewMessageWorkflow)
        {
            this.getAllHotMessagesWorkflow = getAllHotMessagesWorkflow;
            this.addNewMessageWorkflow = chatAddNewMessageWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpPost]
        [Route("messages")]
        public async Task<IActionResult> AddNewNessage(AddNewMessageRequestDto requestDto)
        {
            return new JsonResult(JsonCommonSerializer.SerializeToString(await addNewMessageWorkflow.AddNewMessageAsync(requestDto)));
        }

        [HttpGet]
        [Route("messages/hot")]
        public async Task<IActionResult> GetAllHotMessages(GetHotMessagesRequestDto requestDto)
        {
            return new JsonResult(JsonCommonSerializer.SerializeToString(await getAllHotMessagesWorkflow.GetAllMessages(requestDto)));
        }
    }
}
