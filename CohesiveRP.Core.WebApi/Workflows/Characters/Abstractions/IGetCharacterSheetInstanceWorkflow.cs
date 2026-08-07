using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions
{
    public interface IGetCharacterSheetInstanceWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetCharacterSheetInstancesByChatId(string chatId, string characterId);
    }
}
