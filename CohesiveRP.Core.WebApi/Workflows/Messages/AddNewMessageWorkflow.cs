using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.WebApi.Workflows.Messages;

public class AddNewMessageWorkflow : IChatAddNewMessageWorkflow
{
    private IStorageService storageService;
    private ISummaryService summaryService;

    public AddNewMessageWorkflow(IStorageService storageService, ISummaryService summaryService)
    {
        this.storageService = storageService;
        this.summaryService = summaryService;
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

        var persona = await storageService.GetPersonaByIdAsync(chat.PersonaId);
        if (persona == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Persona with id {chat.PersonaId} was not found."
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
        HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chat.ChatId);
        await AseptisePreviousMessageIfRequiredAsync(chat, persona, characters, hotMessagesDbModel);

        // Add a background query to generate the sceneTracker first and foremost
        // Note: we're not checking up on if the function was successful as this is a soft dependency on the chat roleplay
        if (hotMessagesDbModel != null && hotMessagesDbModel.Messages.Count > 4)
        {
            await AddSceneTrackerBackgroundQueryAsync(chat);
        }

        // Also query the skillChecksInitiator query+
        await AddSkillChecksInitiatorBackgroundQueryAsync(chat);

        IMessageDbModel message = null;
        if (!string.IsNullOrWhiteSpace(requestDto.Message.Content))
        {
            // Add the message
            CreateMessageQueryModel messageQueryModel = new()
            {
                ChatId = requestDto.ChatId,
                Summarized = false,// adding a brand new message, so ofc it's not summarized yet
                SourceType = Common.BusinessObjects.MessageSourceType.User,
                MessageContent = requestDto.Message.Content,
                CreatedAtUtc = DateTime.UtcNow,
                CharacterId = null,// Null as this is from the User
                AvatarId = null,// TODO: generate a different avatar from time to time using comfyui?
            };

            message = await storageService.AddMessageAsync(messageQueryModel);
        }

        // The message was added to storage, we'll query a request for the backend to process a new AI reply
        var backgroundQueryModel = new CreateBackgroundQueryQueryModel
        {
            ChatId = requestDto.ChatId,
            Priority = BackgroundQueryPriority.Highest,// User is waiting!
            DependenciesTags = [BackgroundQuerySystemTags.sceneTracker.ToString(), BackgroundQuerySystemTags.skillChecksInitiator.ToString()],// Can't run as long as another one with one of these tag is running or pending
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

        // Also queue up a summarization background query to process any pending ones with low/very low priority
        GlobalSettingsDbModel globalSettings = await storageService.GetGlobalSettingsAsync();
        _ = summaryService.EvaluateSummaryAsync(chat.ChatId, globalSettings);

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new MessageResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Message = new MessageDefinition
            {
                MessageId = message?.MessageId,
                PersonaId = chat?.PersonaId,
                PersonaName = persona?.Name,
                Summarized = message?.Summarized ?? false,
                AvatarId = message?.AvatarId,
                Content = message?.Content.ReplacePromptBasicPlaceholders(characters.FirstOrDefault(f => f.CharacterId == message.CharacterId)?.Name ?? "(the character)", persona?.Name ?? "User")
            },
            MainQueryId = backgroundQuery.BackgroundQueryId,
        };

        return responseDto;
    }

    private async Task AseptisePreviousMessageIfRequiredAsync(ChatDbModel chat, PersonaDbModel persona, CharacterDbModel[] characters, HotMessagesDbModel hotMessagesDbModel)
    {
        if (hotMessagesDbModel?.Messages == null || hotMessagesDbModel.Messages.Count <= 0)
        {
            return;
        }

        if (hotMessagesDbModel.Messages.Any(a => a.SourceType == Common.BusinessObjects.MessageSourceType.User) || hotMessagesDbModel.Messages.Count >= 20)
        {
            return;
        }

        foreach (var message in hotMessagesDbModel.Messages)
        {
            message.Content = message.Content.ReplacePromptBasicPlaceholders(characters.FirstOrDefault(f => f.CharacterId == message.CharacterId)?.Name ?? "(the character)", persona?.Name ?? "User");
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

    private async Task<bool> AddSkillChecksInitiatorBackgroundQueryAsync(ChatDbModel chat)
    {
        var backgroundQueryModel = new CreateBackgroundQueryQueryModel
        {
            ChatId = chat.ChatId,
            Priority = BackgroundQueryPriority.Highest,// User is waiting!
            DependenciesTags = [],// No dependencies at all
            Tags = [BackgroundQuerySystemTags.skillChecksInitiator.ToString()],
        };

        if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
            return false;

        return true;
    }
}
