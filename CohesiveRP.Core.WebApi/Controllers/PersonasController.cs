using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.Workflows.Chat;
using CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonasController : ControllerBase
    {
        private IGetAllPersonasWorkflow getAllPersonasWorkflow;
        private IGetPersonaByIdWorkflow getPersonaByIdWorkflow;
        private IAddPersonaWorkflow addPersonaWorkflow;
        private IUpdatePersonaWorkflow updatePersonaWorkflow;
        private IDeletePersonaWorkflow deletePersonaWorkflow;
        private IImportAndReplaceAvatarWorkflow importAndReplacePersonaAvatarWorkflow;

        public PersonasController(
            IGetAllPersonasWorkflow getAllPersonasWorkflow,
            IGetPersonaByIdWorkflow getPersonaByIdWorkflow,
            IAddPersonaWorkflow addPersonaWorkflow,
            IUpdatePersonaWorkflow updatePersonaWorkflow,
            IDeletePersonaWorkflow deletePersonaWorkflow,
            IImportAndReplaceAvatarWorkflow importAndReplacePersonaAvatarWorkflow)
        {
            this.getAllPersonasWorkflow = getAllPersonasWorkflow;
            this.getPersonaByIdWorkflow = getPersonaByIdWorkflow;
            this.addPersonaWorkflow = addPersonaWorkflow;
            this.updatePersonaWorkflow = updatePersonaWorkflow;
            this.deletePersonaWorkflow = deletePersonaWorkflow;
            this.importAndReplacePersonaAvatarWorkflow = importAndReplacePersonaAvatarWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        public async Task<IActionResult> GetAllPersonas()
        {
            return new JsonResult(await getAllPersonasWorkflow.GetAllPersonasAsync());
        }

        [HttpGet]
        [Route("{personaId}")]
        public async Task<IActionResult> GetPersonaById(string personaId)
        {
            return new JsonResult(await getPersonaByIdWorkflow.GetPersonaByIdAsync(personaId));
        }

        [HttpPost]
        public async Task<IActionResult> AddPersona(AddNewPersonaRequestDto requestDto)
        {
            return new JsonResult(await addPersonaWorkflow.AddPersonaAsync(requestDto));
        }

        [HttpPut]
        [Route("{personaId}")]
        public async Task<IActionResult> UpdatePersona(UpdatePersonaRequestDto requestDto)
        {
            return new JsonResult(await updatePersonaWorkflow.UpdatePersonaAsync(requestDto));
        }

        [HttpPost]
        [Route("{personaId}/avatar")]
        public async Task<IActionResult> ImportNewAvatar([FromForm] ImportAndReplaceAvatarRequestDto requestDto)
        {
            return new JsonResult(await importAndReplacePersonaAvatarWorkflow.ImportAvatarAsync(requestDto));
        }

        [HttpDelete]
        [Route("{personaId}")]
        public async Task<IActionResult> DeletePersona(DeletePersonaRequestDto requestDto)
        {
            return new JsonResult(await deletePersonaWorkflow.DeletePersonaAsync(requestDto));
        }
    }
}
