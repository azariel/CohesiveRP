using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions
{
    public interface IAddChatCompletionPresetWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> AddChatCompletionPresetAsync(AddChatCompletionPresetRequestDto requestDto);
    }
}
