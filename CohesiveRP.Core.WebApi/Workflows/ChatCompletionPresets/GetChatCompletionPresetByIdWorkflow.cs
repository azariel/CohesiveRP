using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets;
using CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.ChatCompletionPresets.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetChatCompletionPresetByIdWorkflow : IGetChatCompletionPresetByIdWorkflow
{
    private IStorageService storageService;

    public GetChatCompletionPresetByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetChatCompletionPreset(string completionPresetId)
    {
        var chatCompletionPreset = await storageService.GetChatCompletionPresetAsync(completionPresetId);

        if (chatCompletionPreset == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Chat completion preset with id {completionPresetId} was not found."
            };
        }

        var responseDto = new ChatCompletionPresetResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            ChatCompletionPreset = new ChatCompletionPreset
            {
                ChatCompletionPresetId = chatCompletionPreset.ChatCompletionPresetId,
                Name = chatCompletionPreset.Name,
                Format = chatCompletionPreset.Format,
            }
        };

        return responseDto;
    }
}
