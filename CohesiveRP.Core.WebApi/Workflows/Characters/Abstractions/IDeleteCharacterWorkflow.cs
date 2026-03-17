using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions
{
    public interface IDeleteCharacterWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> DeleteCharacterAsync(string characterId);
    }
}
