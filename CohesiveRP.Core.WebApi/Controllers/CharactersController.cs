using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharactersController : ControllerBase
    {
        private IGetAllCharactersWorkflow getAllCharactersWorkflow;
        private IImportNewCharacterWorkflow importNewCharacterWorkflow;

        public CharactersController(
            IGetAllCharactersWorkflow getAllCharactersWorkflow,
            IImportNewCharacterWorkflow importNewCharacterWorkflow)
        {
            this.getAllCharactersWorkflow = getAllCharactersWorkflow;
            this.importNewCharacterWorkflow = importNewCharacterWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        public async Task<IActionResult> GetAllCharacters()
        {
            return new JsonResult(await getAllCharactersWorkflow.GetAllCharactersAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ImportNewCharacter([FromForm] ImportNewCharacterRequestDto requestDto)
        {
            return new JsonResult(await importNewCharacterWorkflow.ImportNewCharacterAsync(requestDto));
        }
    }
}
