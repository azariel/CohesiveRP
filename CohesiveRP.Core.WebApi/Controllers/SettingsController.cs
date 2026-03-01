using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private IGetGlobalSettingsWorkflow getGlobalSettingsWorkflow;

        public SettingsController(
            IGetGlobalSettingsWorkflow getGlobalSettingsWorkflow)
        {
            this.getGlobalSettingsWorkflow = getGlobalSettingsWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        public async Task<IActionResult> GetGlobalSettings()
        {
            return new JsonResult(JsonCommonSerializer.SerializeToString(await getGlobalSettingsWorkflow.GetGlobalSettings()));
        }
    }
}
