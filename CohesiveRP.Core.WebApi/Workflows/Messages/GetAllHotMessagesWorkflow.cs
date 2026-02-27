using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetAllHotMessagesWorkflow : IGetAllHotMessagesWorkflow
{
    private IStorageService storageService;

    public GetAllHotMessagesWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetAllMessages(GetHotMessagesRequestDto requestDto)
    {
        var messages = await storageService.GetAllHotMessages(requestDto.ChatId);

        messages ??= Array.Empty<MessageDbModel>();
        var responseDto = new MessagesResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Messages = messages.Select(s => new MessageDefinition
            {
                MessageId = s.MessageId,
                Content = s.Content,
            }).ToArray(),
        };

        return responseDto;
    }
}
