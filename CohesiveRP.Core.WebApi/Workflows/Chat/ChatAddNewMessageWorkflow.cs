using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class ChatAddNewMessageWorkflow : IChatAddNewMessageWorkflow
{
    private IStorageService storageService;

    public ChatAddNewMessageWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiReponseDto> AddNewMessageAsync(GetChatByIdRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentException.ThrowIfNullOrWhiteSpace(requestDto.ChatId);

        // Validate that the chat exists in storage
        var chat = await storageService.GetChatAsync(requestDto.ChatId);
        return chat;
        //if (chat is not ChatResponseDto _chatResponseDto)
        //{
        //    return chat;
        //}

        //return chat;
    }
}
