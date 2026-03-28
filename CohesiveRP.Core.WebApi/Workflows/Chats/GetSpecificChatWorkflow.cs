using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chats
{
    public class GetSpecificChatWorkflow : IGetSpecificChatWorkflow
    {
        private IStorageService storageService;

        public GetSpecificChatWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IWebApiResponseDto> GetChatById(string chatId)
        {
            ChatDbModel chat = await storageService.GetChatAsync(chatId);

            if (chat == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.NotFound,
                    Message = $"Chat with id {chatId} was not found."
                };
            }

            var characterId = chat.CharacterIds?.FirstOrDefault();
            var character = await storageService.GetCharacterByIdAsync(characterId);
            ChatResponseDto responseDto = new()
            {
                ChatId = chatId,
                ChatName = chat.Name,
                AvatarFilePath = chat.AvatarFilePath,
                LastActivityAtUtc = chat.LastActivityAtUtc,
                CharacterIds = chat.CharacterIds,
                LorebookIds = chat.LorebookIds,
                PersonaId  = chat.PersonaId,
                HttpResultCode = System.Net.HttpStatusCode.OK,
            };

            return responseDto;
        }
    }
}
