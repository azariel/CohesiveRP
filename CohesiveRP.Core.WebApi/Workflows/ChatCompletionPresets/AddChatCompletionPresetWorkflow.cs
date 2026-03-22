using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class AddChatCompletionPresetWorkflow : IAddChatCompletionPresetWorkflow
{
    private IStorageService storageService;

    public AddChatCompletionPresetWorkflow (IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> AddChatCompletionPresetAsync(AddChatCompletionPresetRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentNullException.ThrowIfNull(requestDto.ChatCompletionRequest);
        ArgumentNullException.ThrowIfNull(requestDto.ChatCompletionRequest.Name);
        ArgumentNullException.ThrowIfNull(requestDto.ChatCompletionRequest.Format);

        ChatCompletionPresetsDbModel dbModel = new()
        {
            Name = requestDto.ChatCompletionRequest.Name,
            Format = requestDto.ChatCompletionRequest.Format,
        };

        var result = await storageService.AddChatCompletionPresetAsync(dbModel);
        if (result == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"ChatCompletionPreset creation failed."
            };
        }

        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
