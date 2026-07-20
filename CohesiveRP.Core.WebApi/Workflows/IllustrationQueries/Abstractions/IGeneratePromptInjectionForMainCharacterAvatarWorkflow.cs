using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.Workflows.IllustrationQueries.Abstractions
{
    public interface IGeneratePromptInjectionForMainCharacterAvatarWorkflow: IWorkflow
    {
        Task<IWebApiResponseDto> Generate(GeneratePromptInjectionForCharacterIllustrationRequestDto requestDto);
    }
}
