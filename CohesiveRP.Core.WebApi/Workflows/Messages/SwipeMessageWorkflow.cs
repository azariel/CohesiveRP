using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class SwipeMessageWorkflow : ISwipeMessageWorkflow
{
    private IStorageService storageService;
    private IChatAddNewMessageWorkflow addNewMessageWorkflow;

    public SwipeMessageWorkflow(IStorageService storageService, IChatAddNewMessageWorkflow addNewMessageWorkflow)
    {
        this.storageService = storageService;
        this.addNewMessageWorkflow = addNewMessageWorkflow;
    }

    public async Task<IWebApiResponseDto> SwipeMessageAsync(GetSpecificMessageRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentException.ThrowIfNullOrWhiteSpace(requestDto.ChatId);

        // Validate that the chat exists in storage
        var chat = await storageService.GetChatAsync(requestDto.ChatId);
        if (chat == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Chat with id {requestDto.ChatId} was not found."
            };
        }

        var persona = await storageService.GetPersonaByIdAsync(chat.PersonaId);
        if (persona == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Persona with id {chat.PersonaId} was not found."
            };
        }

        IMessageDbModel message = await storageService.GetSpecificMessageAsync(requestDto.ChatId, requestDto.MessageId);

        if (message == null)
        {
            LoggingManager.LogToFile("f5ccb25e-653b-4618-966f-98230cfc33e7", $"Couldn't get message from id [{requestDto.MessageId}] in chat [{requestDto.ChatId}]. Message was not found.");
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Message not found."
            };
        }

        // invalidate the message by simply deleting it in hotMessages
        var deletionResult = await storageService.DeleteSpecificMessageAsync(requestDto.ChatId, requestDto.MessageId);

        if (!deletionResult)
        {
            LoggingManager.LogToFile("f5ccb25e-653b-4618-966f-98230cfc33e7", $"Failed to delete message with id [{requestDto.MessageId}] in chat [{requestDto.ChatId}].");
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Message swipe failed. Failed to delete the message in storage so that we could regenerate it."
            };
        }

        var hotMessages = await storageService.GetAllHotMessagesAsync(requestDto.ChatId);
        message = hotMessages.Messages.MaxBy(m => m.CreatedAtUtc);

        // The message was added to storage, we'll query a request for the backend to process a new AI reply
        var backgroundQueryModel = new CreateBackgroundQueryQueryModel
        {
            ChatId = requestDto.ChatId,
            Priority = BackgroundQueryPriority.Highest,// User is waiting!
            DependenciesTags = [
                BackgroundQuerySystemTags.sceneTracker.ToString(),
                BackgroundQuerySystemTags.skillChecksInitiator.ToString(),
                BackgroundQuerySystemTags.proseGuardian.ToString(),
                BackgroundQuerySystemTags.narrativeDirection.ToString(),
            ],// Can't run as long as another one with one of these tag is running or pending
            Tags = [BackgroundQuerySystemTags.main.ToString()],// This is a message from the player and thus is tagged as 'main'
        };

        var backgroundQuery = await storageService.AddBackgroundQueryAsync(backgroundQueryModel);// Note that we're still not querying the LLM at this point, we're adding a query to be process async against the backend and that process will eventually query the LLMs

        var characters = await storageService.GetCharactersAsync();
        var responseDto = new MessageResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Message = new MessageDefinition
            {
                MessageId = message?.MessageId,
                PersonaId = chat?.PersonaId,
                PersonaName = persona?.Name,
                InRoleplayDateTime = message?.InRoleplayDateTime,
                Summarized = message?.Summarized ?? false,
                CharacterAvatars = message?.CharacterAvatars,
                ThinkingContent = message?.ThinkingContent,
                Content = message?.Content.ReplacePromptBasicPlaceholders(characters.FirstOrDefault(f => f.CharacterId == message.CharacterId)?.Name ?? "(the character)", persona?.Name ?? "User"),
                StartGenerationDateTimeUtc = message?.StartGenerationDateTimeUtc,
                StartFocusedGenerationDateTimeUtc = message?.StartFocusedGenerationDateTimeUtc,
                EndFocusedGenerationDateTimeUtc = message?.EndFocusedGenerationDateTimeUtc,
            },
            MainQueryId = backgroundQuery.BackgroundQueryId,
        };

        return responseDto;
    }
}
