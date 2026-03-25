using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions
{
    public interface IDeleteChatCompletionPresetWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> DeleteChatCompletionPresetAsync(DeleteChatCompletionPresetRequestDto requestDto);
    }
}
