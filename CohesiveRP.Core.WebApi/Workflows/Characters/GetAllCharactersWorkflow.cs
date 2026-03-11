using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetAllCharactersWorkflow : IGetAllCharactersWorkflow
{
    private IStorageService storageService;

    public GetAllCharactersWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public Task<IWebApiResponseDto> GetAllCharactersAsync()
    {
        //requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        //ArgumentException.ThrowIfNullOrWhiteSpace(requestDto.ChatId);

        //// Validate that the chat exists in storage
        //var chat = await storageService.GetChatAsync(requestDto.ChatId);

        //if (chat == null)
        //{
        //    return new WebApiException
        //    {
        //        HttpResultCode = System.Net.HttpStatusCode.NotFound,
        //        Message = $"Chat with id {requestDto.ChatId} was not found."
        //    };
        //}

        //// Add a background query to generate the sceneTracker first and foremost
        //// Note: we're not checking up on if the function was successful as this is a soft dependency on the chat roleplay
        //await AddSceneTrackerBackgroundQueryAsync(chat);

        //// Add the message
        //CreateMessageQueryModel messageQueryModel = new()
        //{
        //    ChatId = requestDto.ChatId,
        //    Summarized = false,// adding a brand new message, so ofc it's not summarized yet
        //    SourceType = Common.BusinessObjects.MessageSourceType.User,
        //    MessageContent = requestDto.Message.Content,
        //    CreatedAtUtc = DateTime.UtcNow,
        //};

        //var message = await storageService.CreateMessageAsync(messageQueryModel);

        //// The message was added to storage, we'll query a request for the backend to process a new AI reply
        //var backgroundQueryModel = new CreateBackgroundQueryQueryModel
        //{
        //    ChatId = requestDto.ChatId,
        //    Priority = BackgroundQueryPriority.Highest,// User is waiting!
        //    DependenciesTags = [BackgroundQuerySystemTags.sceneTracker.ToString()],// Can't run as long as another one with one of these tag is running or pending
        //    Tags = [BackgroundQuerySystemTags.main.ToString()],// This is a message from the player and thus is tagged as 'main'
        //};

        //var backgroundQuery = await storageService.CreateBackgroundQueryAsync(backgroundQueryModel);// Note that we're still not querying the LLM at this point, we're adding a query to be process async against the backend and that process will eventually query the LLMs

        //// Convert DbModel to an acceptable web model (without sensitive information)
        //var responseDto = new MessageResponseDto
        //{
        //    HttpResultCode = System.Net.HttpStatusCode.OK,
        //    Message = new MessageDefinition
        //    {
        //        MessageId = message.MessageId,
        //        Summarized = message.Summarized,
        //        Content = message.Content,
        //    },
        //    MainQueryId = backgroundQuery.BackgroundQueryId,
        //};

        //return responseDto;
        return null;
    }
}
