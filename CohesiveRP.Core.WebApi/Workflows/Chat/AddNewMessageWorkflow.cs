using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class AddNewMessageWorkflow : IChatAddNewMessageWorkflow
{
    private IStorageService storageService;

    public AddNewMessageWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> AddNewMessageAsync(AddNewMessageRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentException.ThrowIfNullOrWhiteSpace(requestDto.ChatId);

        // Validate that the chat exists in storage
        var chat = await storageService.GetChatAsync(requestDto.ChatId);

        if (chat == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Chat with id {requestDto.ChatId} was not found."
            };
        }

        if (!DateTime.TryParse(requestDto.Message.timestampUtc, out DateTime messageDate))
        {
            messageDate = DateTime.UtcNow;
        }

        // Add the message
        CreateMessageQueryModel queryModel = new()
        {
            ChatId = requestDto.ChatId,
            MessageContent = requestDto.Message.Content,
            TimestampUtc = messageDate,
        };

        var message = await storageService.CreateMessageAsync(queryModel);

        // Get all messages
        //var messages = await storageService.GetAllHotMessages(requestDto.ChatId);

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new MessageResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Message =  new MessageDefinition
            {
                MessageId = message.MessageId,
                Content = message.Content,
            },
        };

        return responseDto;
    }
}
