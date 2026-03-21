using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetAllHotMessagesWorkflow : IGetAllHotMessagesWorkflow
{
    private IStorageService storageService;

    public GetAllHotMessagesWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetAllMessages(GetHotMessagesRequestDto requestDto)
    {
        var chat = await storageService.GetChatAsync(requestDto.ChatId);
        var characters = await storageService.GetCharactersAsync();
        HotMessagesDbModel messagesDbModel = await storageService.GetAllHotMessagesAsync(requestDto.ChatId);

        if(messagesDbModel == null)
        {
            LoggingManager.LogToFile("4fcf8cab-2137-470e-a916-ea274ed86301", $"Couldn't get hot messages in chat [{requestDto.ChatId}]. Message were not found.");
            return null;
        }

        messagesDbModel.Messages ??= new();

        var persona = await storageService.GetPersonaByIdAsync(chat.PersonaId);
        List<MessageDefinition> messageDefinitions = new List<MessageDefinition>();
        for (int i = 0; i < messagesDbModel.Messages.Count; i++)
        {
            var characterName = characters.FirstOrDefault(f => f.CharacterId == messagesDbModel.Messages[i].CharacterId)?.Name;
            messageDefinitions.Add(new MessageDefinition
            {
                MessageId = messagesDbModel.Messages[i].MessageId,
                MessageIndex = messagesDbModel.NbColdMessages + i + 1,
                Content = messagesDbModel.Messages[i].Content.ReplacePromptBasicPlaceholders(characterName ?? "(the character)", persona?.Name ?? "User"),
                SourceType = messagesDbModel.Messages[i].SourceType,
                Summarized = messagesDbModel.Messages[i].Summarized,
                CreatedAtUtc = messagesDbModel.Messages[i].CreatedAtUtc,
                CharacterId = messagesDbModel.Messages[i].CharacterId,
                AvatarId = messagesDbModel.Messages[i].AvatarId,
                CharacterName = characterName,
                PersonaId = chat.PersonaId,
                PersonaName = persona?.Name ?? "User",
                
            });
        }

        var responseDto = new MessagesResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Messages = messageDefinitions.ToArray(),
            NbColdMessages = messagesDbModel.NbColdMessages,
        };

            return responseDto;
        }
    }
