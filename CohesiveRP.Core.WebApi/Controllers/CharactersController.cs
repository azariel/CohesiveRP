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
        private IGetCharacterByIdWorkflow getCharacterByIdWorkflow;
        private IImportNewCharacterWorkflow importNewCharacterWorkflow;
        private IUpdateCharacterWorkflow updateCharacterWorkflow;
        private IDeleteCharacterWorkflow deleteCharacterWorkflow;

        public CharactersController(
            IGetAllCharactersWorkflow getAllCharactersWorkflow,
            IGetCharacterByIdWorkflow getCharacterByIdWorkflow,
            IUpdateCharacterWorkflow updateCharacterWorkflow,
            IDeleteCharacterWorkflow deleteCharacterWorkflow,
            IImportNewCharacterWorkflow importNewCharacterWorkflow)
        {
            this.getAllCharactersWorkflow = getAllCharactersWorkflow;
            this.getCharacterByIdWorkflow = getCharacterByIdWorkflow;
            this.importNewCharacterWorkflow = importNewCharacterWorkflow;
            this.updateCharacterWorkflow = updateCharacterWorkflow;
            this.deleteCharacterWorkflow = deleteCharacterWorkflow;
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

        [HttpPut]
        [Route("{characterId}")]
        public async Task<IActionResult> UpdateCharacter(UpdateCharacterRequestDto requestDto)
        {
            return new JsonResult(await updateCharacterWorkflow.UpdateCharacterAsync(requestDto));
        }

        [HttpDelete]
        [Route("{characterId}")]
        public async Task<IActionResult> DeleteCharacter(string characterId)
        {
            return new JsonResult(await deleteCharacterWorkflow.DeleteCharacterAsync(characterId));
        }

        [HttpPost]
        public async Task<IActionResult> ImportNewCharacter([FromForm] ImportNewCharacterRequestDto requestDto)
        {
            return new JsonResult(await importNewCharacterWorkflow.ImportNewCharacterAsync(requestDto));
        }
    }
}
