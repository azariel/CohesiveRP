using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.Users;

namespace CohesiveRP.Core.WebApi.Workflows.Chat
{
    public class CreateNewChatWorkflow : ICreateNewChatWorkflow
    {
        private IStorageService storageService;

        public CreateNewChatWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IWebApiResponseDto> AddNewChatAsync(AddNewChatRequestDto requestDto)
        {
            CreateChatQueryModel queryModel = new()
            {
            };

            ChatDbModel newlyCreatedChat = await storageService.CreateChatAsync(queryModel);

            if (newlyCreatedChat == null)
            {
                // TODO: creation failed...
            }

            return new ChatResponseDto
            {
                ChatId = newlyCreatedChat.ChatId,
                HttpResultCode = System.Net.HttpStatusCode.OK,
            };
        }
    }
}
