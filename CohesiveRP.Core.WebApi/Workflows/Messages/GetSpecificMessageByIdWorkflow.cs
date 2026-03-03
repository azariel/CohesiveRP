using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetSpecificMessageByIdWorkflow : IGetSpecificMessageByIdWorkflow
{
    private IStorageService storageService;

    public GetSpecificMessageByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetSpecificMessage(GetSpecificMessageRequestDto requestDto)
    {
        IMessageDbModel message = await storageService.GetSpecificMessageAsync(requestDto.ChatId, requestDto.MessageId);

        if(message == null)
        {
            return null;
        }

        var responseDto = new MessageResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Message = new MessageDefinition
            {
                MessageId = message.MessageId,
                Content = message.Content,
                SourceType = message.SourceType,
                CreatedAtUtc = message.CreatedAtUtc,
            },
            MainQueryId = null
        };

        return responseDto;
    }
}
