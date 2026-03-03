using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.Users;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetAllSelectableChatsWorkflow : IGetAllSelectableChatsWorkflow
{
    private IStorageService storageService;

    public GetAllSelectableChatsWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetAllSelectableChats()
    {
        ChatDbModel[] chats = await storageService.GetAllChatsAsync();

        chats ??= Array.Empty<ChatDbModel>();
        ChatDefinitionsResponseDto responseDto = new();

        foreach (var chat in chats)
        {
            responseDto.Chats.Add(new ChatDefinition
            {
                ChatId = chat.ChatId,
            });
        }

        responseDto.HttpResultCode = System.Net.HttpStatusCode.OK;
        return responseDto;
    }
}
