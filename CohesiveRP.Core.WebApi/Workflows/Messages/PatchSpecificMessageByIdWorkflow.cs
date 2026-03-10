using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class PatchSpecificMessageByIdWorkflow : IPatchSpecificMessageByIdWorkflow
{
    private IStorageService storageService;

    public PatchSpecificMessageByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> PatchSpecificMessage(PatchSpecificMessageRequestDto requestDto)
    {
        MessageDbModel message = await storageService.GetSpecificMessageAsync(requestDto.ChatId, requestDto.Message.MessageId) as MessageDbModel;

        if (message == null)
        {
            //CreateMessageQueryModel queryModel = new CreateMessageQueryModel
            //{
            //    ChatId = requestDto.ChatId,
            //    CreatedAtUtc = DateTime.UtcNow,
            //    MessageContent = requestDto.Message.Content,
            //    SourceType = requestDto.Message.
            //};

            //message = await storageService.CreateMessageAsync(queryModel) as MessageDbModel;
            LoggingManager.LogToFile("99189f9e-1a04-4360-b57c-edc1768eff4a", $"Couldn't get message [{requestDto.Message.MessageId}] from chat [{requestDto.ChatId}]. Couldn't update a missing message.");
            return null;
        } else
        {
            // We ONLY accept modification of the content, nothing else
            message.Content = requestDto.Message.Content;
            if (!await storageService.UpdateHotMessageAsync(requestDto.ChatId, message))
            {
                LoggingManager.LogToFile("ee8aea84-8d5a-43d3-b0b8-98ec5b460634", $"Couldn't update message [{requestDto.Message.MessageId}] from chat [{requestDto.ChatId}].");
                return null;
            }
        }

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
