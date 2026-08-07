using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Characters.CharacterSheetInstances;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions
{
    public interface IUpdateCharacterSheetInstanceWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> UpdateCharacterSheetInstanceAsync(UpdateCharacterSheetInstanceRequestDto requestDto);
    }
}
