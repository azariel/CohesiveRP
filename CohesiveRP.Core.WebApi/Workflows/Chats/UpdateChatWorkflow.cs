using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chats
{
    public class UpdateChatWorkflow : IUpdateChatWorkflow
    {
        private IStorageService storageService;

        public UpdateChatWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IWebApiResponseDto> UpdateChatAsync(UpdateChatRequestDto requestDto)
        {
            if (requestDto?.ChatId == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                    Message = $"Chat update request dto was malformed."
                };
            }

            ChatDbModel chat = await storageService.GetChatAsync(requestDto.ChatId);
            if (chat == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.NotFound,
                    Message = $"Chat with id {requestDto.ChatId} was not found. Aborting update request."
                };
            }

            // validate that the tethered characters exists
            if (requestDto.CharacterIds != null && requestDto.CharacterIds.Count > 0)
            {
                var characters = await storageService.GetCharactersAsync();
                var characterIds = characters.Select(s => s.CharacterId).ToArray();
                var missingCharacters = requestDto.CharacterIds.Where(a => !characterIds.Any(an => an.Equals(a, StringComparison.InvariantCultureIgnoreCase))).ToArray();
                if (missingCharacters.Length > 0)
                {
                    return new WebApiException
                    {
                        HttpResultCode = System.Net.HttpStatusCode.NotFound,
                        Message = $"Couldn't update chat with id {requestDto.ChatId}. Characters [{string.Join(",", missingCharacters)}] were not found in storage."
                    };
                }
            }

            // validate that the tethered lorebooks exists
            if (requestDto.LorebookIds != null && requestDto.LorebookIds.Count > 0)
            {
                var lorebooks = await storageService.GetLorebooksAsync();
                var lorebookIds = lorebooks.Select(s => s.LorebookId).ToArray();
                var missingLorebooks = requestDto.LorebookIds.Where(a => !lorebookIds.Any(an => an.Equals(a, StringComparison.InvariantCultureIgnoreCase))).ToArray();
                if (missingLorebooks.Length > 0)
                {
                    return new WebApiException
                    {
                        HttpResultCode = System.Net.HttpStatusCode.NotFound,
                        Message = $"Couldn't update chat with id {requestDto.ChatId}. Lorebooks [{string.Join(",", missingLorebooks)}] were not found in storage."
                    };
                }
            }

            // Update the chat
            chat.Name = requestDto.Name;
            chat.PersonaId = requestDto.PersonaId;
            chat.CharacterIds = requestDto.CharacterIds;
            chat.LorebookIds = requestDto.LorebookIds;
            var updateChatResult = await storageService.UpdateChatAsync(chat);
            if (!updateChatResult)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"Couldn't update chat with id {requestDto.ChatId} in storage."
                };
            }

            ChatResponseDto responseDto = new()
            {
                ChatId = chat.ChatId,
                LorebookIds = chat.LorebookIds,
                CharacterIds = chat.CharacterIds,
                LastActivityAtUtc = chat.LastActivityAtUtc,
                ChatName = chat.Name,
                PersonaId = chat.PersonaId,
                HttpResultCode = System.Net.HttpStatusCode.OK,
            };

            return responseDto;
        }
    }
}
