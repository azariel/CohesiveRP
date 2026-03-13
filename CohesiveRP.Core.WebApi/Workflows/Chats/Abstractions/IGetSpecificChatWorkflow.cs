using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions
{
    public interface IGetSpecificChatWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetChatById(string chatId);
    }
}
