using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions
{
    public interface IGetLorebookByIdWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetLorebookByIdAsync(string lorebookId);
    }
}
