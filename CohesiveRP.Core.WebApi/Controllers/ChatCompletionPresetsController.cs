using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatCompletionPresetsController : ControllerBase
    {
        private IChatCompletionPresetsWorkflow getChatCompletionPresetsWorkflow;
        private IGetChatCompletionPresetByIdWorkflow getChatCompletionPresetByIdWorkflow;
        private IAddChatCompletionPresetWorkflow addChatCompletionPresetWorkflow;
        private IUpdateChatCompletionPresetWorkflow updateChatCompletionPresetWorkflow;
        private IDeleteChatCompletionPresetWorkflow deleteChatCompletionPresetWorkflow;

        public ChatCompletionPresetsController(
            IChatCompletionPresetsWorkflow getChatCompletionPresetsWorkflow,
            IGetChatCompletionPresetByIdWorkflow getChatCompletionPresetByIdWorkflow,
            IAddChatCompletionPresetWorkflow addChatCompletionPresetWorkflow,
            IUpdateChatCompletionPresetWorkflow updateChatCompletionPresetWorkflow,
            IDeleteChatCompletionPresetWorkflow deleteChatCompletionPresetWorkflow)
        {
            this.getChatCompletionPresetsWorkflow = getChatCompletionPresetsWorkflow;
            this.getChatCompletionPresetByIdWorkflow = getChatCompletionPresetByIdWorkflow;
            this.addChatCompletionPresetWorkflow = addChatCompletionPresetWorkflow;
            this.updateChatCompletionPresetWorkflow = updateChatCompletionPresetWorkflow;
            this.deleteChatCompletionPresetWorkflow = deleteChatCompletionPresetWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        public async Task<IActionResult> GetChatCompletionPresets()
        {
            return new JsonResult(await getChatCompletionPresetsWorkflow.GetChatCompletionPresets());
        }

        [HttpGet]
        [Route("{completionPresetId}")]
        public async Task<IActionResult> GetChatCompletionPreset(string completionPresetId)
        {
            return new JsonResult(await getChatCompletionPresetByIdWorkflow.GetChatCompletionPreset(completionPresetId));
        }

        [HttpPost]
        public async Task<IActionResult> AddChatCompletionPreset(AddChatCompletionPresetRequestDto requestDto)
        {
            return new JsonResult(await addChatCompletionPresetWorkflow.AddChatCompletionPresetAsync(requestDto));
        }

        [HttpPut]
        [Route("{chatCompletionPresetId}")]
        public async Task<IActionResult> UpdateChatCompletionPreset(UpdateChatCompletionPresetRequestDto requestDto)
        {
            return new JsonResult(await updateChatCompletionPresetWorkflow.UpdateChatCompletionPresetAsync(requestDto));
        }

        [HttpDelete]
        [Route("{chatCompletionPresetId}")]
        public async Task<IActionResult> DeleteChatCompletionPreset(DeleteChatCompletionPresetRequestDto requestDto)
        {
            return new JsonResult(await deleteChatCompletionPresetWorkflow.DeleteChatCompletionPresetAsync(requestDto));
        }
    }
}
