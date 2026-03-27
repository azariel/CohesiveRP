using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets.Abstractions
{
    public interface IGetCharacterSheetWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetCharacterSheetByCharacterIdAsync(string characterId);
        Task<IWebApiResponseDto> GetCharacterSheetByPersonaIdAsync(string personaId);
    }
}
