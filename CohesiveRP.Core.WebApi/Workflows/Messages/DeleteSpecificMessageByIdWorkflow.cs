using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class DeleteSpecificMessageByIdWorkflow : IDeleteSpecificMessageByIdWorkflow
{
    private IStorageService storageService;

    public DeleteSpecificMessageByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> DeleteSpecificMessage(GetSpecificMessageRequestDto requestDto)
    {
        bool success= await storageService.DeleteSpecificMessageAsync(requestDto.ChatId, requestDto.MessageId);

        if(!success)
        {
            LoggingManager.LogToFile("af1fbce7-d5d9-4247-aebe-021e161961b6", $"Couldn't delete message from id [{requestDto.MessageId}] in chat [{requestDto.ChatId}].");
            return null;
        }

        return new DeleteMessageResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };
    }
}
