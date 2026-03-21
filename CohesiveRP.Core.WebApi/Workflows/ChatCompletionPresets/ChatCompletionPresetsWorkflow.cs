using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets;
using CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class ChatCompletionPresetsWorkflow : IChatCompletionPresetsWorkflow
{
    private IStorageService storageService;

    public ChatCompletionPresetsWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetChatCompletionPresets()
    {
        var chatCompletionPresets = await storageService.GetChatCompletionPresetAsync();
        var responseDto = new ChatCompletionPresetsResponseDto
        {
            ChatCompletionPresetsMap = chatCompletionPresets?.Select(s=> new ChatCompletionAllPresets
            {
                ChatCompletionPresetId = s.ChatCompletionPresetId,
                Name = s.Name,
            })?.ToArray() ?? [],
        };

        responseDto.HttpResultCode = System.Net.HttpStatusCode.OK;
        return responseDto;
    }
}
