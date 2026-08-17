using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions
{
    public interface IGetCharactersByChatIdWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetCharactersByChatIdAsync(string chatId);
    }
}
