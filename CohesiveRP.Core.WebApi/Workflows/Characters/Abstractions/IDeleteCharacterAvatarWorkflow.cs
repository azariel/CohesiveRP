using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions
{
    public interface IDeleteCharacterAvatarWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> DeleteCharacterAvatarAsync(string characterId, string avatarFileName);
    }
}
