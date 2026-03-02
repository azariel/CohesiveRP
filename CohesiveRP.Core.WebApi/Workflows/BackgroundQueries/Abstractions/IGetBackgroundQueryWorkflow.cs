using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions
{
    public interface IGetBackgroundQueryWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetBackgroundQuery(string queryId);
    }
}
