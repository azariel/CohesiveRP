using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class DeleteChatCompletionPresetWorkflow : IDeleteChatCompletionPresetWorkflow
{
    private IStorageService storageService;

    public DeleteChatCompletionPresetWorkflow (IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> DeleteChatCompletionPresetAsync(DeleteChatCompletionPresetRequestDto requestDto)
    {
        ArgumentNullException.ThrowIfNull(requestDto?.ChatCompletionPresetId);

        var currentChatCompletionPreset = await storageService.GetChatCompletionPresetAsync(requestDto.ChatCompletionPresetId);
        if (currentChatCompletionPreset == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"ChatCompletionPreset [{requestDto.ChatCompletionPresetId}] to delete couldn't be found in storage."
            };
        }

        bool result = await storageService.DeleteChatCompletionPresetAsync(currentChatCompletionPreset.ChatCompletionPresetId);
        if (!result)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"ChatCompletionPreset [{requestDto.ChatCompletionPresetId}] deletion failed."
            };
        }

        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
