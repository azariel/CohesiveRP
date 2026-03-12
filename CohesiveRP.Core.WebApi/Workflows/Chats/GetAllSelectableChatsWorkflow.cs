using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chats;

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

        // TODO: pagination
        var characters = await storageService.GetCharactersAsync();
        foreach (var chat in chats)
        {
            responseDto.Chats.Add(new ChatDefinition
            {
                ChatId = chat.ChatId,
                CharacterId = chat.CharacterIds?.FirstOrDefault() ?? "unknown",
                ChatName = characters.FirstOrDefault(f => f.CharacterId == chat.CharacterIds?.FirstOrDefault())?.Name ?? "unknown",
            });
        }

        responseDto.HttpResultCode = System.Net.HttpStatusCode.OK;
        return responseDto;
    }
}
