using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions
{
    public interface IGetCharacterByIdWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetCharacterByIdAsync(string characterId);
    }
}
