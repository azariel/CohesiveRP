using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/{chatId}")]
    public class ChatController : ControllerBase
    {
        private IGetAllHotMessagesWorkflow getAllHotMessagesWorkflow;
        private IGetSpecificMessageByIdWorkflow getSpecificMessageByIdWorkflow;
        private IDeleteSpecificMessageByIdWorkflow deleteSpecificMessageByIdWorkflow;
        private IPatchSpecificMessageByIdWorkflow putSpecificMessageByIdWorkflow;
        private IChatAddNewMessageWorkflow addNewMessageWorkflow;

        private IGetPromptByChatIdWorkflow getPromptByChatIdWorkflow;

        public ChatController(
            IGetAllHotMessagesWorkflow getAllHotMessagesWorkflow,
            IGetSpecificMessageByIdWorkflow getSpecificMessageByIdWorkflow,
            IDeleteSpecificMessageByIdWorkflow deleteSpecificMessageByIdWorkflow,
            IPatchSpecificMessageByIdWorkflow putSpecificMessageByIdWorkflow,
            IChatAddNewMessageWorkflow chatAddNewMessageWorkflow,
            IGetPromptByChatIdWorkflow getPromptByChatIdWorkflow)
        {
            this.getAllHotMessagesWorkflow = getAllHotMessagesWorkflow;
            this.getSpecificMessageByIdWorkflow = getSpecificMessageByIdWorkflow;
            this.deleteSpecificMessageByIdWorkflow = deleteSpecificMessageByIdWorkflow;
            this.putSpecificMessageByIdWorkflow = putSpecificMessageByIdWorkflow;
            this.addNewMessageWorkflow = chatAddNewMessageWorkflow;
            this.getPromptByChatIdWorkflow = getPromptByChatIdWorkflow;
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

        [HttpDelete]
        [Route("messages/{messageId}")]
        public async Task<IActionResult> DeleteSpecificMessageById(GetSpecificMessageRequestDto requestDto)
        {
            return new JsonResult(await deleteSpecificMessageByIdWorkflow.DeleteSpecificMessage(requestDto));
        }

        [HttpPut]
        [Route("messages/{messageId}")]
        public async Task<IActionResult> PutSpecificMessageById(PatchSpecificMessageRequestDto requestDto)
        {
            return new JsonResult(await putSpecificMessageByIdWorkflow.PatchSpecificMessage(requestDto));
        }

        // Prompt
        [HttpGet]
        [Route("prompt")]
        public async Task<IActionResult> GetPrompt([FromRoute] string chatId)
        {
            return new JsonResult(await getPromptByChatIdWorkflow.GeneratePromptForChatId(chatId, BackgroundQuerySystemTags.main.ToString()));
        }
    }
}
