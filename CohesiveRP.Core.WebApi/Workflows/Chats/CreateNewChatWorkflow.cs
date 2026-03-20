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
            if (string.IsNullOrWhiteSpace(requestDto?.CharacterId))
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                    Message = $"Chat creation failed. The specified characterId was null or empty."
                };
            }

            PersonaDbModel[] personas = await storageService.GetPersonasAsync();
            PersonaDbModel defaultPersona = personas?.FirstOrDefault(w => w.IsDefault);

            if (defaultPersona == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                    Message = $"There is no default Persona in storage. Please select one before creating a new chat."
                };
            }

            CreateChatQueryModel queryModel = new()
            {
                Name = string.IsNullOrWhiteSpace(requestDto.CharacterId) ? "New chat" : requestDto.CharacterId,
                SelectedChatCompletionPresets = null,
                PersonaId = defaultPersona.PersonaId,
                CharacterIds = [requestDto.CharacterId],
                LorebookIds = requestDto.LorebookIds,
            };

            ChatDbModel newlyCreatedChat = await storageService.AddChatAsync(queryModel);

            if (newlyCreatedChat == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"Chat creation failed in storage."
                };
            }

            // Add the chat default Avatar (same as the character's)
            if (newlyCreatedChat.CharacterIds != null && newlyCreatedChat.CharacterIds.Count > 0)
            {
                string characterAvatarFilePath = Path.Combine(WebConstants.CharactersAvatarFilePath, newlyCreatedChat.CharacterIds.First(), WebConstants.AvatarFileName);
                if (File.Exists(characterAvatarFilePath))
                {
                    string chatDirectoryPath = Path.Combine(WebConstants.ChatsAvatarFilePath, newlyCreatedChat.ChatId);
                    if (!Directory.Exists(chatDirectoryPath))
                    {
                        Directory.CreateDirectory(chatDirectoryPath);
                    }

                    File.Copy(characterAvatarFilePath, Path.Combine(chatDirectoryPath, WebConstants.AvatarFileName));
                }
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
                        await storageService.UpdateCharacterAsync(characterToUpdate);
                    } catch (Exception) { } // nothing, just skip
                }
            }

            return new ChatCreationResponseDto
            {
                ChatId = newlyCreatedChat.ChatId,
                ChatName = newlyCreatedChat.Name,
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
                AvatarId = null,
            };
            await storageService.AddMessageAsync(queryModel);
        }
    }
}
