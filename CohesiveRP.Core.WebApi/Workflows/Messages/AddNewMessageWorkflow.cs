using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.WebApi.Workflows.Messages;

public class AddNewMessageWorkflow : IChatAddNewMessageWorkflow
{
    private IStorageService storageService;

    public AddNewMessageWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> AddNewMessageAsync(AddNewMessageRequestDto requestDto)
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

        // If a background with main tag is already running, forbid adding a new message until we're done
        var backgroundQueriesInProgress = await storageService.GetPendingOrProcessingBackgroundQueryAsync();
        if (backgroundQueriesInProgress.Any(a => a.Tags.Contains(BackgroundQuerySystemTags.main.ToString())))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Chat with id {requestDto.ChatId} is already in the process of generating a reply. Please wait."
            };
        }

        var characters = await storageService.GetCharactersAsync();

        // if it's the first player message in the chat, aseptise the previous messages
        await AseptisePreviousMessageIfRequiredAsync(chat, characters);

        IMessageDbModel message = null;
        if (!string.IsNullOrWhiteSpace(requestDto.Message.Content))
        {
            // Add a background query to generate the sceneTracker first and foremost
            // Note: we're not checking up on if the function was successful as this is a soft dependency on the chat roleplay
            await AddSceneTrackerBackgroundQueryAsync(chat);

            // Add the message
            CreateMessageQueryModel messageQueryModel = new()
            {
                ChatId = requestDto.ChatId,
                Summarized = false,// adding a brand new message, so ofc it's not summarized yet
                SourceType = Common.BusinessObjects.MessageSourceType.User,
                MessageContent = requestDto.Message.Content,
                CreatedAtUtc = DateTime.UtcNow,
                CharacterId = null,// Null as this is from the User
            };

            message = await storageService.AddMessageAsync(messageQueryModel);
        }

        // The message was added to storage, we'll query a request for the backend to process a new AI reply
        var backgroundQueryModel = new CreateBackgroundQueryQueryModel
        {
            ChatId = requestDto.ChatId,
            Priority = BackgroundQueryPriority.Highest,// User is waiting!
            DependenciesTags = [BackgroundQuerySystemTags.sceneTracker.ToString()],// Can't run as long as another one with one of these tag is running or pending
            Tags = [BackgroundQuerySystemTags.main.ToString()],// This is a message from the player and thus is tagged as 'main'
        };

        var backgroundQuery = await storageService.AddBackgroundQueryAsync(backgroundQueryModel);// Note that we're still not querying the LLM at this point, we're adding a query to be process async against the backend and that process will eventually query the LLMs

        // Update the LastActivity field on the characters linked to this chat so that we can order them in the UI upon request
        if (chat.CharacterIds != null)
        {
            foreach (string characterId in chat.CharacterIds)
            {
                try
                {
                    var characterToUpdate = await storageService.GetCharacterByIdAsync(characterId);
                    characterToUpdate.LastActivityAtUtc = DateTime.UtcNow;
                    await storageService.UpdateCharacterAsync(characterToUpdate);
                } catch (Exception) { } // nothing, just skip
            }
        }

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new MessageResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Message = new MessageDefinition
            {
                MessageId = message?.MessageId,
                Summarized = message?.Summarized ?? false,
                Content = message?.Content.ReplacePromptBasicPlaceholders(characters.FirstOrDefault(f => f.CharacterId == message.CharacterId)?.Name ?? "(the character)", "Azariel")
            },
            MainQueryId = backgroundQuery.BackgroundQueryId,
        };

        return responseDto;
    }

    private async Task AseptisePreviousMessageIfRequiredAsync(ChatDbModel chat, CharacterDbModel[] characters)
    {
        var messages = await storageService.GetAllHotMessagesAsync(chat.ChatId);

        if (messages == null || messages.Length <= 0)
        {
            return;
        }

        if (messages.Any(a => a.SourceType == Common.BusinessObjects.MessageSourceType.User) || messages.Length >= 20)
        {
            return;
        }

        foreach (var message in messages)
        {
            message.Content = message.Content.ReplacePromptBasicPlaceholders(characters.FirstOrDefault(f => f.CharacterId == message.CharacterId)?.Name ?? "(the character)", "Azariel");
            await storageService.UpdateHotMessageAsync(chat.ChatId, (MessageDbModel)message);
        }
    }

    private async Task<bool> AddSceneTrackerBackgroundQueryAsync(ChatDbModel chat)
    {
        var backgroundQueryModel = new CreateBackgroundQueryQueryModel
        {
            ChatId = chat.ChatId,
            Priority = BackgroundQueryPriority.Highest,// User is waiting!
            DependenciesTags = [],// No dependencies at all
            Tags = [BackgroundQuerySystemTags.sceneTracker.ToString()],
        };

        if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
            return false;

        return true;
    }
}
