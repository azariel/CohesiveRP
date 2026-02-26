using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Storage;
using CohesiveRP.Core.WebApi.Services;

namespace CohesiveRP.Core.WebApi.Workflows.Chat
{
    public class ChatAddNewMessageWorkflow : IChatAddNewMessageWorkflow
    {
        private IStorageService storageService;

        public ChatAddNewMessageWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<string> AddNewMessageAsync(GetChatByIdRequestDto requestDto)
        {
            requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
            ArgumentException.ThrowIfNullOrWhiteSpace(requestDto.ChatId);

            // Validate that the chat exists in storage
            ChatResponseDto chat = await storageService.GetChatAsync(requestDto.ChatId);

            if (chat == null)
            {
                return "Chat does not exists.";
            }

            // TODO: replace this placeholder
            return $"Message added to chat with ID: {requestDto.ChatId}";
        }
    }
}
