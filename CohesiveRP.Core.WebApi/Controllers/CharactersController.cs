using CohesiveRP.Common.Exceptions;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets.Abstractions;
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
        private IGetCharacterSheetWorkflow getCharacterSheetWorkflow;
        private IAddCharacterSheetWorkflow addCharacterSheetWorkflow;
        private IUpdateCharacterSheetWorkflow updateCharacterSheetWorkflow;
        private IRegenerateCharacterSheetWorkflow regenerateCharacterSheetWorkflow;
        private IExportCharacterCardWorkflow exportCharacterCardWorkflow;
        private IImportCharacterCardWorkflow importCharacterCardWorkflow;
        private IDeleteCharacterAvatarWorkflow deleteCharacterAvatarWorkflow;

        public CharactersController(
            IGetAllCharactersWorkflow getAllCharactersWorkflow,
            IGetCharacterByIdWorkflow getCharacterByIdWorkflow,
            IUpdateCharacterWorkflow updateCharacterWorkflow,
            IDeleteCharacterWorkflow deleteCharacterWorkflow,
            IImportNewCharacterWorkflow importNewCharacterWorkflow,
            IGetCharacterSheetWorkflow getCharacterSheetWorkflow,
            IAddCharacterSheetWorkflow addCharacterSheetWorkflow,
            IUpdateCharacterSheetWorkflow updateCharacterSheetWorkflow,
            IRegenerateCharacterSheetWorkflow regenerateCharacterSheetWorkflow,
            IExportCharacterCardWorkflow exportCharacterCardWorkflow,
            IImportCharacterCardWorkflow importCharacterCardWorkflow,
            IDeleteCharacterAvatarWorkflow deleteCharacterAvatarWorkflow)
        {
            this.getAllCharactersWorkflow = getAllCharactersWorkflow;
            this.getCharacterByIdWorkflow = getCharacterByIdWorkflow;
            this.importNewCharacterWorkflow = importNewCharacterWorkflow;
            this.updateCharacterWorkflow = updateCharacterWorkflow;
            this.deleteCharacterWorkflow = deleteCharacterWorkflow;
            this.getCharacterSheetWorkflow = getCharacterSheetWorkflow;
            this.addCharacterSheetWorkflow = addCharacterSheetWorkflow;
            this.updateCharacterSheetWorkflow = updateCharacterSheetWorkflow;
            this.regenerateCharacterSheetWorkflow = regenerateCharacterSheetWorkflow;
            this.exportCharacterCardWorkflow = exportCharacterCardWorkflow;
            this.importCharacterCardWorkflow = importCharacterCardWorkflow;
            this.deleteCharacterAvatarWorkflow = deleteCharacterAvatarWorkflow;
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

        // ---------- Character Sheet ----------

        [HttpGet]
        [Route("{characterId}/charactersheet")]
        public async Task<IActionResult> GetCharacterSheetByCharacterId(string characterId)
        {
            return new JsonResult(await getCharacterSheetWorkflow.GetCharacterSheetByCharacterIdAsync(characterId));
        }

        [HttpGet]
        [Route("personaCharacterSheet/{personaId}")]
        public async Task<IActionResult> GetCharacterSheetByPersonaId(string personaId)
        {
            return new JsonResult(await getCharacterSheetWorkflow.GetCharacterSheetByPersonaIdAsync(personaId));
        }

        [HttpPost]
        [Route("{characterId}/charactersheet")]
        public async Task<IActionResult> CreateCharacterSheet(AddCharacterSheetRequestDto requestDto)
        {
            return new JsonResult(await addCharacterSheetWorkflow.AddCharacterSheetAsync(requestDto));
        }

        [HttpPost]
        [Route("{characterId}/charactersheet/{characterSheetId}/regenerate")]
        public async Task<IActionResult> RegenerateCharacterSheet(RegenerateCharacterSheetRequestDto requestDto)
        {
            return new JsonResult(await regenerateCharacterSheetWorkflow.RegenerateCharacterSheetAsync(requestDto));
        }

        [HttpPost]
        [Route("personaCharacterSheet/{personaId}/regenerate")]
        public async Task<IActionResult> RegeneratePersonaCharacterSheet(RegenerateCharacterSheetRequestDto requestDto)
        {
            return new JsonResult(await regenerateCharacterSheetWorkflow.RegenerateCharacterSheetAsync(requestDto));
        }

        [HttpPut]
        [Route("{characterId}/charactersheet/{characterSheetId}")]
        public async Task<IActionResult> UpdateCharacterSheet(UpdateCharacterSheetRequestDto requestDto)
        {
            return new JsonResult(await updateCharacterSheetWorkflow.UpdateCharacterSheetAsync(requestDto));
        }

        [HttpGet]
        [Route("{characterId}/exportCharacterCard")]
        public async Task<IActionResult> ExportCharacterCard(string characterId)
        {
            var result = await exportCharacterCardWorkflow.ExportCharacterCard(characterId);
            if (result is WebApiException exception)
            {
                return StatusCode((int)exception.HttpResultCode, exception.Message);
            }

            if (result is ExportCRPV1ResponseDto dto)
            {
                return File(dto.Image, "image/png", $"{characterId}.png");
            }

            return StatusCode(500, "Unexpected error during export.");
        }

        [HttpPost]
        [Route("{characterId}/importCharacterCard")]
        public async Task<IActionResult> ImportCharacterCard([FromRoute] string characterId, [FromForm] ImportNewCharacterRequestDto requestDto)
        {
            return new JsonResult(await importCharacterCardWorkflow.ImportCharacterAsync(characterId, requestDto));
        }

        // Avatars
        [HttpDelete]
        [Route("{characterId}/avatars/{avatarFileName}")]
        public async Task<IActionResult> DeleteCharacterAvatar([FromRoute] string characterId, [FromRoute] string avatarFileName)
        {
            return new JsonResult(await deleteCharacterAvatarWorkflow.DeleteCharacterAvatarAsync(characterId, avatarFileName));
        }
    }
}
