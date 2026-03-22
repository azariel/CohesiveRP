using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions
{
    public interface IGetChatCompletionPresetByIdWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetChatCompletionPreset(string completionPresetId);
    }
}
