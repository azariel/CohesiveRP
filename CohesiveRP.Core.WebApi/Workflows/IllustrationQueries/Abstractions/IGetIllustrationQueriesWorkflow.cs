using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.IllustrationQueries.Abstractions
{
    public interface IGetIllustrationQueriesWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetQueries(string chatId);
    }
}
