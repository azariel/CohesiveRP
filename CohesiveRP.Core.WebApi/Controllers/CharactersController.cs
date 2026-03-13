using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Chat;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharactersController : ControllerBase
    {
        private IGetAllCharactersWorkflow getAllCharactersWorkflow;
        private IGetCharacterByIdWorkflow getCharacterByIdWorkflow;
        private IImportNewCharacterWorkflow importNewCharacterWorkflow;

        public CharactersController(
            IGetAllCharactersWorkflow getAllCharactersWorkflow,
            IGetCharacterByIdWorkflow getCharacterByIdWorkflow,
            IImportNewCharacterWorkflow importNewCharacterWorkflow)
        {
            this.getAllCharactersWorkflow = getAllCharactersWorkflow;
            this.getCharacterByIdWorkflow = getCharacterByIdWorkflow;
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

        [HttpGet]
        [Route("{characterId}")]
        public async Task<IActionResult> GetCharacterById(string characterId)
        {
            return new JsonResult(await getCharacterByIdWorkflow.GetCharacterByIdAsync(characterId));
        }

        [HttpPost]
        public async Task<IActionResult> ImportNewCharacter([FromForm] ImportNewCharacterRequestDto requestDto)
        {
            return new JsonResult(await importNewCharacterWorkflow.ImportNewCharacterAsync(requestDto));
        }
    }
}
