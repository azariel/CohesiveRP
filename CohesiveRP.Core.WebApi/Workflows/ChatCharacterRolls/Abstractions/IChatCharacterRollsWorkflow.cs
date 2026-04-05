using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.ChatCharacterRolls.Abstractions
{
    public interface IChatCharacterRollsWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetChatCharacterRolls(string chatId);
    }
}
