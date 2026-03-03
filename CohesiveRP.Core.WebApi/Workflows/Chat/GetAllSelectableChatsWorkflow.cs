using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

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
                //ChatCompletionPresets = chat.ChatCompletionPresets,
            });
        }

        responseDto.HttpResultCode = System.Net.HttpStatusCode.OK;
        return responseDto;
    }
}
