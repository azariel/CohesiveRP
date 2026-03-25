using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions
{
    public interface IUpdateGlobalSettingsWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> UpdateGlobalSettings(UpdateGlobalSettingsRequestDto requestDto);
    }
}
