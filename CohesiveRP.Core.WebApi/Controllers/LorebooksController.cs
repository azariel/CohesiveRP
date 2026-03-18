using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LorebooksController : ControllerBase
    {
        private IGetAllLorebooksWorkflow getAllLorebooksWorkflow;
        private IGetLorebookByIdWorkflow getLorebookByIdWorkflow;
        private IAddLorebookWorkflow addLorebookWorkflow;
        private IUpdateLorebookWorkflow updateLorebookWorkflow;
        private IDeleteLorebookWorkflow deleteLorebookWorkflow;
        private IImportLorebookWorkflow importLorebookWorkflow;
        private IImportAndReplaceLorebookAvatarWorkflow importAndReplaceLorebookAvatarWorkflow;

        public LorebooksController(
            IGetAllLorebooksWorkflow getAllLorebooksWorkflow,
            IGetLorebookByIdWorkflow getLorebookByIdWorkflow,
            IAddLorebookWorkflow addLorebookWorkflow,
            IUpdateLorebookWorkflow updateLorebookWorkflow,
            IDeleteLorebookWorkflow deleteLorebookWorkflow,
            IImportLorebookWorkflow importLorebookWorkflow,
            IImportAndReplaceLorebookAvatarWorkflow importAndReplaceLorebookAvatarWorkflow)
        {
            this.getAllLorebooksWorkflow = getAllLorebooksWorkflow;
            this.getLorebookByIdWorkflow = getLorebookByIdWorkflow;
            this.addLorebookWorkflow = addLorebookWorkflow;
            this.updateLorebookWorkflow = updateLorebookWorkflow;
            this.deleteLorebookWorkflow = deleteLorebookWorkflow;
            this.importLorebookWorkflow = importLorebookWorkflow;
            this.importAndReplaceLorebookAvatarWorkflow = importAndReplaceLorebookAvatarWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        public async Task<IActionResult> GetAllLorebooks()
        {
            return new JsonResult(await getAllLorebooksWorkflow.GetAllLorebooksAsync());
        }

        [HttpGet]
        [Route("{lorebookId}")]
        public async Task<IActionResult> GetLorebookById(string lorebookId)
        {
            return new JsonResult(await getLorebookByIdWorkflow.GetLorebookByIdAsync(lorebookId));
        }

        [HttpPost]
        public async Task<IActionResult> AddLorebook(AddNewLorebookRequestDto requestDto)
        {
            return new JsonResult(await addLorebookWorkflow.AddLorebookAsync(requestDto));
        }

        [HttpPut]
        [Route("{lorebookId}")]
        public async Task<IActionResult> UpdateLorebook(UpdateLorebookRequestDto requestDto)
        {
            return new JsonResult(await updateLorebookWorkflow.UpdateLorebookAsync(requestDto));
        }

        [HttpPost]
        [Route("import")]
        public async Task<IActionResult> ImportLorebook([FromForm] ImportLorebookRequestDto requestDto)
        {
            return new JsonResult(await importLorebookWorkflow.ImportAsync(requestDto));
        }

        [HttpPost]
        [Route("{lorebookId}/avatar")]
        public async Task<IActionResult> ImportNewAvatar([FromForm] ImportAndReplaceLorebookAvatarRequestDto requestDto)
        {
            return new JsonResult(await importAndReplaceLorebookAvatarWorkflow.ImportAvatarAsync(requestDto));
        }

        [HttpDelete]
        [Route("{lorebookId}")]
        public async Task<IActionResult> DeleteLorebook(DeleteLorebookRequestDto requestDto)
        {
            return new JsonResult(await deleteLorebookWorkflow.DeleteLorebookAsync(requestDto));
        }
    }
}
