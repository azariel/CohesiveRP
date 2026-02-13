using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.WebApi.ResponseDtos;
using CohesiveRP.Storage.WebApi.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUsersWorkflow usersWorkflow;

        public UsersController(IUsersWorkflow workflow)
        {
            usersWorkflow = workflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        [Route("create")]
        public async Task<IActionResult> CreateNewUser(UserCreationRequestDto userRequestDto) => new JsonResult(JsonCommonSerializer.SerializeToString(usersWorkflow.CreateNewUser(userRequestDto)));
    }
}
