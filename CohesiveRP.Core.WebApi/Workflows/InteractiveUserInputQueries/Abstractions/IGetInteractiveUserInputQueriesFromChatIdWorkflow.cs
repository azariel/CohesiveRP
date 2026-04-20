using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.InteractiveUserInputQueries.Abstractions
{
    public interface IGetInteractiveUserInputQueriesFromChatIdWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetInteractiveUserInputQueryAsync(string chatId);
    }
}
