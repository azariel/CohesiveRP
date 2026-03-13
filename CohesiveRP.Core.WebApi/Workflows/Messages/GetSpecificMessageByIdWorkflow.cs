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
            LoggingManager.LogToFile("52d724a4-a17a-4f4c-9ab1-6f79c02d5490", $"Couldn't get message from id [{requestDto.MessageId}] in chat [{requestDto.ChatId}]. Message was not found.");
            return null;
        }

        var character = await storageService.GetCharacterByIdAsync(message.CharacterId);
        var responseDto = new MessageResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Message = new MessageDefinition
            {
                MessageId = message.MessageId,
                Content = message.Content.ReplacePromptBasicPlaceholders(character?.Name ?? "(the character)", "Azariel"),
                SourceType = message.SourceType,
                Summarized = message.Summarized,
                CreatedAtUtc = message.CreatedAtUtc,
                CharacterId = message.CharacterId,
            },
            MainQueryId = null
        };

        return responseDto;
    }
}
