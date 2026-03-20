using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions
{
    public interface IDeleteSpecificChatWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> DeleteChatById(string chatId);
    }
}
