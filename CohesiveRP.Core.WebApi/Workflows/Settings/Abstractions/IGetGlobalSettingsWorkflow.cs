using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions
{
    public interface IGetGlobalSettingsWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetGlobalSettings();
    }
}
