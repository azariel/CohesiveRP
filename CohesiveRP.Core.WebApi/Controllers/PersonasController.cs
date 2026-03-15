using CohesiveRP.Core.WebApi.RequestDtos.Chat;
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

        public PersonasController(
            IGetAllPersonasWorkflow getAllPersonasWorkflow,
            IGetPersonaByIdWorkflow getPersonaByIdWorkflow,
            IAddPersonaWorkflow addPersonaWorkflow)
        {
            this.getAllPersonasWorkflow = getAllPersonasWorkflow;
            this.getPersonaByIdWorkflow = getPersonaByIdWorkflow;
            this.addPersonaWorkflow = addPersonaWorkflow;
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
    }
}
