using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions
{
    public interface IChatCompletionPresetsWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetChatCompletionPresets();
    }
}
