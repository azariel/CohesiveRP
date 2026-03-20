using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private IGetAllSelectableChatsWorkflow getAllChatsWorkflow;
        private IGetSpecificChatWorkflow getSpecificChat;
        private ICreateNewChatWorkflow createNewChatWorkflow;
        private IDeleteSpecificChatWorkflow deleteSpecificChat;

        public ChatsController(
            IGetAllSelectableChatsWorkflow getAllChatsWorkflow,
            IGetSpecificChatWorkflow getSpecificChat,
            ICreateNewChatWorkflow createNewChatWorkflow,
            IDeleteSpecificChatWorkflow deleteSpecificChat)
        {
            this.getAllChatsWorkflow = getAllChatsWorkflow;
            this.getSpecificChat = getSpecificChat;
            this.createNewChatWorkflow = createNewChatWorkflow;
            this.deleteSpecificChat = deleteSpecificChat;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        public async Task<IActionResult> GetAllSelectableChats([FromQuery] string characterId)
        {
            return new JsonResult(await getAllChatsWorkflow.GetAllSelectableChats(characterId));
        }

        [HttpGet]
        [Route("{chatId}")]
        public async Task<IActionResult> GetSpecificChatById(string chatId)
        {
            return new JsonResult(await getSpecificChat.GetChatById(chatId));
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewChat(AddNewChatRequestDto requestDto)
        {
            return new JsonResult(await createNewChatWorkflow.AddNewChatAsync(requestDto));
        }

        [HttpDelete]
        [Route("{chatId}")]
        public async Task<IActionResult> DeleteSpecificChatById(string chatId)
        {
            return new JsonResult(await deleteSpecificChat.DeleteChatById(chatId));
        }
    }
}
