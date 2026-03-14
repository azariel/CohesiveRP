using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions
{
    public interface IGetBackgroundQueriesByChatIdWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetBackgroundQueries(string chatId);
    }
}
