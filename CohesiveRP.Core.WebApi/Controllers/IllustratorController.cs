using CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries;
using CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.IllustrationQueries.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IllustratorController : ControllerBase
    {
        private IGetIllustrationQueriesWorkflow getIllustrationQueries;
        private IAddIllustrationQueryWorkflow addIllustrationQuery;
        private IGeneratePromptInjectionForMainCharacterAvatarWorkflow generatePromptInjectionForMainCharacterAvatar;

        public IllustratorController(
            IGetIllustrationQueriesWorkflow getIllustrationQueries,
            IAddIllustrationQueryWorkflow addIllustrationQuery,
            IGeneratePromptInjectionForMainCharacterAvatarWorkflow generatePromptInjectionForMainCharacterAvatar)
        {
            this.getIllustrationQueries = getIllustrationQueries;
            this.addIllustrationQuery = addIllustrationQuery;
            this.generatePromptInjectionForMainCharacterAvatar = generatePromptInjectionForMainCharacterAvatar;
        }

        [HttpGet]
        [Route("imateapot")]
        public async Task<IActionResult> GetImateapot() => new JsonResult("You're a teapot.");

        // Prompt Injection
        [HttpPost]
        [Route("promptInjection")]
        public async Task<IActionResult> GeneratePromptInjectionForMainCharacterAvatar(GeneratePromptInjectionForCharacterIllustrationRequestDto requestDto)
        {
            return new JsonResult(await generatePromptInjectionForMainCharacterAvatar.Generate(requestDto));
        }

        // Queries
        [HttpGet]
        [Route("queries")]
        public async Task<IActionResult> GetIllustrationQueries([FromRoute] string chatId)
        {
            return new JsonResult(await getIllustrationQueries.GetQueries(chatId));
        }

        [HttpPost]
        [Route("queries")]
        public async Task<IActionResult> AddIllustrationQueries(AddIllustrationQueryRequestDto requestDto)
        {
            return new JsonResult(await addIllustrationQuery.AddQuery(requestDto));
        }
    }
}
