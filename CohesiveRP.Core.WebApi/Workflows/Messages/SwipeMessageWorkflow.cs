using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class SwipeMessageWorkflow : ISwipeMessageWorkflow
{
    private IStorageService storageService;
    private IChatAddNewMessageWorkflow addNewMessageWorkflow;

    public SwipeMessageWorkflow(IStorageService storageService, IChatAddNewMessageWorkflow addNewMessageWorkflow)
    {
        this.storageService = storageService;
        this.addNewMessageWorkflow = addNewMessageWorkflow;
    }

    public async Task<IWebApiResponseDto> SwipeMessageAsync(GetSpecificMessageRequestDto requestDto)
    {
        IMessageDbModel message = await storageService.GetSpecificMessageAsync(requestDto.ChatId, requestDto.MessageId);

        if (message == null)
        {
            LoggingManager.LogToFile("f5ccb25e-653b-4618-966f-98230cfc33e7", $"Couldn't get message from id [{requestDto.MessageId}] in chat [{requestDto.ChatId}]. Message was not found.");
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Message not found."
            };
        }

        // invalidate the message by simply deleting it in hotMessages
        var deletionResult = await storageService.DeleteSpecificMessageAsync(requestDto.ChatId, requestDto.MessageId);

        if (!deletionResult)
        {
            LoggingManager.LogToFile("f5ccb25e-653b-4618-966f-98230cfc33e7", $"Failed to delete message with id [{requestDto.MessageId}] in chat [{requestDto.ChatId}].");
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Message swipe failed. Failed to delete the message in storage so that we could regenerate it."
            };
        }
        
        var addNewMessageResponseDto = await addNewMessageWorkflow.AddNewMessageAsync(new AddNewMessageRequestDto
        {
            ChatId = requestDto.ChatId,
            QueueDependentBackgroundTasks = false, // we want to regenerate ONLY the message, not the skill checks, sceneTracker and what not. This is a swipe, not a delete + retry, which is a different behavior
            Message = new()
            {
                Content = message.Content,
            },
        });

        return addNewMessageResponseDto;
    }
}
