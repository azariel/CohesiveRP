using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class PutSpecificMessageByIdWorkflow : IPutSpecificMessageByIdWorkflow
{
    private IStorageService storageService;

    public PutSpecificMessageByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> PutSpecificMessage(PutSpecificMessageRequestDto requestDto)
    {
        MessageDbModel message = await storageService.GetSpecificMessageAsync(requestDto.ChatId, requestDto.Message.MessageId) as MessageDbModel;

        if(message == null)
        {
            return null;
        }

        // We ONLY accept modification of the content, nothing else
        message.Content = requestDto.Message.Content;
        await storageService.UpdateHotMessageAsync(requestDto.ChatId, message);

        var responseDto = new MessageResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Message = new MessageDefinition
            {
                MessageId = message.MessageId,
                Content = message.Content,
                SourceType = message.SourceType,
                Summarized = message.Summarized,
                CreatedAtUtc = message.CreatedAtUtc,
            },
            MainQueryId = null
        };

        return responseDto;
    }
}
