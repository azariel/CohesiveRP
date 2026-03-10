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
        private IGetSpecificMessageByIdWorkflow getSpecificMessageByIdWorkflow;
        private IPatchSpecificMessageByIdWorkflow putSpecificMessageByIdWorkflow;
        private IChatAddNewMessageWorkflow addNewMessageWorkflow;

        public ChatController(
            IGetAllHotMessagesWorkflow getAllHotMessagesWorkflow,
            IGetSpecificMessageByIdWorkflow getSpecificMessageByIdWorkflow,
            IPatchSpecificMessageByIdWorkflow putSpecificMessageByIdWorkflow,
            IChatAddNewMessageWorkflow chatAddNewMessageWorkflow)
        {
            this.getAllHotMessagesWorkflow = getAllHotMessagesWorkflow;
            this.getSpecificMessageByIdWorkflow = getSpecificMessageByIdWorkflow;
            this.putSpecificMessageByIdWorkflow = putSpecificMessageByIdWorkflow;
            this.addNewMessageWorkflow = chatAddNewMessageWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpPost]
        [Route("messages")]
        public async Task<IActionResult> AddNewNessage(AddNewMessageRequestDto requestDto)
        {
            return new JsonResult(await addNewMessageWorkflow.AddNewMessageAsync(requestDto));
        }

        [HttpGet]
        [Route("messages/hot")]
        public async Task<IActionResult> GetAllHotMessages(GetHotMessagesRequestDto requestDto)
        {
            return new JsonResult(await getAllHotMessagesWorkflow.GetAllMessages(requestDto));
        }

        [HttpGet]
        [Route("messages/{messageId}")]
        public async Task<IActionResult> GetSpecificMessageById(GetSpecificMessageRequestDto requestDto)
        {
            return new JsonResult(await getSpecificMessageByIdWorkflow.GetSpecificMessage(requestDto));
        }

        [HttpPut]
        [Route("messages/{messageId}")]
        public async Task<IActionResult> PutSpecificMessageById(PatchSpecificMessageRequestDto requestDto)
        {
            return new JsonResult(await putSpecificMessageByIdWorkflow.PatchSpecificMessage(requestDto));
        }
    }
}
