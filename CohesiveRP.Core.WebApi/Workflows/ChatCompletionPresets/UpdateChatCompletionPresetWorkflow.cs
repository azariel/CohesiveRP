using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class UpdateChatCompletionPresetWorkflow : IUpdateChatCompletionPresetWorkflow
{
    private IStorageService storageService;

    public UpdateChatCompletionPresetWorkflow (IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> UpdateChatCompletionPresetAsync(UpdateChatCompletionPresetRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentNullException.ThrowIfNull(requestDto.ChatCompletionPresetId);
        ArgumentNullException.ThrowIfNull(requestDto.ChatCompletionPreset);
        ArgumentNullException.ThrowIfNull(requestDto.ChatCompletionPreset.Name);
        ArgumentNullException.ThrowIfNull(requestDto.ChatCompletionPreset.Format);

        var currentChatCompletionPreset = await storageService.GetChatCompletionPresetAsync(requestDto.ChatCompletionPresetId);
        if (currentChatCompletionPreset == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"ChatCompletionPreset [{requestDto.ChatCompletionPresetId}] to update couldn't be found in storage."
            };
        }

        // Overridable fields
        currentChatCompletionPreset.Name = requestDto.ChatCompletionPreset.Name;
        currentChatCompletionPreset.Format = requestDto.ChatCompletionPreset.Format;

        bool result = await storageService.UpdateChatCompletionPresetAsync(currentChatCompletionPreset);
        if (!result)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"ChatCompletionPreset [{requestDto.ChatCompletionPresetId}] update failed."
            };
        }

        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
