using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private IGetGlobalSettingsWorkflow getGlobalSettingsWorkflow;
        private IUpdateGlobalSettingsWorkflow updateGlobalSettingsWorkflow;

        public SettingsController(
            IGetGlobalSettingsWorkflow getGlobalSettingsWorkflow,
            IUpdateGlobalSettingsWorkflow updateGlobalSettingsWorkflow)
        {
            this.getGlobalSettingsWorkflow = getGlobalSettingsWorkflow;
            this.updateGlobalSettingsWorkflow = updateGlobalSettingsWorkflow;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        [HttpGet]
        public async Task<IActionResult> GetGlobalSettings()
        {
            return new JsonResult(await getGlobalSettingsWorkflow.GetGlobalSettings());
        }

        [HttpPut]
        public async Task<IActionResult> UpdateGlobalSettings(UpdateGlobalSettingsRequestDto requestDto)
        {
            return new JsonResult(await updateGlobalSettingsWorkflow.UpdateGlobalSettings(requestDto));
        }
    }
}
