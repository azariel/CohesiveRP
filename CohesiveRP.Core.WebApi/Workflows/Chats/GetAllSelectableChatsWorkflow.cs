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

    public async Task<IWebApiResponseDto> GetAllSelectableChats(string characterId)
    {
        ChatDbModel[] chats = await storageService.GetAllChatsAsync();
        chats ??= Array.Empty<ChatDbModel>();

        if (!string.IsNullOrWhiteSpace(characterId))
        {
            chats = chats.Where(w => w.CharacterIds != null && w.CharacterIds.Contains(characterId)).ToArray();
        }

        ChatDefinitionsResponseDto responseDto = new();

        // TODO: pagination
        foreach (var chat in chats)
        {
            responseDto.Chats.Add(new ChatDefinition
            {
                ChatId = chat.ChatId,
                CharacterIds = chat.CharacterIds,
                LorebookIds = chat.LorebookIds,
                ChatName = chat.Name,
                LastActivityAtUtc = chat.LastActivityAtUtc,
            });
        }

        responseDto.Chats = responseDto.Chats.OrderByDescending(o => o.LastActivityAtUtc).ToList();
        responseDto.HttpResultCode = System.Net.HttpStatusCode.OK;
        return responseDto;
    }
}
