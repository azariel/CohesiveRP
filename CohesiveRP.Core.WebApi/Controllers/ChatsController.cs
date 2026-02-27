using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private IGetAllSelectableChatsWorkflow getAllChatsWorkflow;
        private ICreateNewChatWorkflow createNewChatWorkflow;

        public ChatsController(
            IGetAllSelectableChatsWorkflow getAllChatsWorkflow,
            ICreateNewChatWorkflow createNewChatWorkflow)
        {
            this.getAllChatsWorkflow = getAllChatsWorkflow;
            this.createNewChatWorkflow = createNewChatWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        public async Task<IActionResult> GetAllSelectableChats()
        {
            return new JsonResult(JsonCommonSerializer.SerializeToString(await getAllChatsWorkflow.GetAllSelectableChats()));
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewChat(AddNewChatRequestDto requestDto)
        {
            return new JsonResult(JsonCommonSerializer.SerializeToString(await createNewChatWorkflow.AddNewChatAsync(requestDto)));
        }
    }
}
