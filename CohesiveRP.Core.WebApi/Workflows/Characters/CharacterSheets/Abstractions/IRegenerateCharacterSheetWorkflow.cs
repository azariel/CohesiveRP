using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets.Abstractions
{
    public interface IRegenerateCharacterSheetWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> RegenerateCharacterSheetAsync(RegenerateCharacterSheetRequestDto requestDto);
    }
}
