using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.WebApi.Workflows.Chats
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
                SelectedChatCompletionPresets = null
            };

            // TEMP
            var chars = await storageService.GetCharactersAsync();
            if (chars.Length > 0)
            {
                queryModel.CharacterIds = [chars.OrderByDescending(o => o.LastActivityAtUtc).First().CharacterId];
            }
            //---------

            ChatDbModel newlyCreatedChat = await storageService.AddChatAsync(queryModel);

            if (newlyCreatedChat == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"Chat creation failed."
                };
            }

            // Add first message from AI
            List<CharacterDbModel> tiedCharacters = await AddFirstChatMessage(newlyCreatedChat);

            // Update characters lastActivity field
            if (newlyCreatedChat.CharacterIds != null)
            {
                foreach (string characterId in newlyCreatedChat.CharacterIds)
                {
                    try
                    {
                        var characterToUpdate = await storageService.GetCharacterByIdAsync(characterId);
                        characterToUpdate.LastActivityAtUtc = DateTime.UtcNow;
                        await storageService.UpdateCharacter(characterToUpdate);
                    } catch (Exception) { } // nothing, just skip
                }
            }

            return new ChatResponseDto
            {
                ChatId = newlyCreatedChat.ChatId,
                ChatName = tiedCharacters.FirstOrDefault()?.Name,
                CharacterId = tiedCharacters.FirstOrDefault()?.CharacterId,
                HttpResultCode = System.Net.HttpStatusCode.OK,
            };
        }

        private async Task<List<CharacterDbModel>> AddFirstChatMessage(ChatDbModel newlyCreatedChat)
        {
            List<CharacterDbModel> tiedCharacters = new();
            if (newlyCreatedChat.CharacterIds != null && newlyCreatedChat.CharacterIds.Count > 0)
            {
                if (newlyCreatedChat.CharacterIds.Count == 1)
                {
                    var character = await storageService.GetCharacterByIdAsync(newlyCreatedChat.CharacterIds[0]);

                    if (character != null)
                        tiedCharacters.Add(character);

                    await AddFirstChatMessageStandardChat(newlyCreatedChat.ChatId, character);
                } else
                {
                    var characters = await storageService.GetCharactersAsync();
                    characters = characters?.Where(w => characters.Any(a => a.CharacterId == w.CharacterId)).ToArray();

                    if (characters != null && characters.Length > 0)
                        tiedCharacters.AddRange(characters);

                    await AddFirstChatMessageGroupChat(newlyCreatedChat.ChatId, characters);
                }
            }

            return tiedCharacters;
        }

        private async Task AddFirstChatMessageGroupChat(string chatId, CharacterDbModel[] characters)
        {
            // TODO
            await AddFirstChatMessageStandardChat(chatId, characters.FirstOrDefault());
        }

        private async Task AddFirstChatMessageStandardChat(string chatId, CharacterDbModel character)
        {
            var queryModel = new CreateMessageQueryModel
            {
                ChatId = chatId,
                CreatedAtUtc = DateTime.UtcNow,
                SourceType = Common.BusinessObjects.MessageSourceType.AI,
                Summarized = false,
                MessageContent = character.FirstMessage,
                CharacterId = character.CharacterId,
            };
            await storageService.AddMessageAsync(queryModel);
        }
    }
}
