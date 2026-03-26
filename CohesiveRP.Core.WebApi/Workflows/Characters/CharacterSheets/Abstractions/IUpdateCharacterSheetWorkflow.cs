using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets.Abstractions
{
    public interface IUpdateCharacterSheetWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> UpdateCharacterSheetAsync(UpdateCharacterSheetRequestDto requestDto);
    }
}
