using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.PromptContext;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.Prompt;
using CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetPromptByChatIdWorkflow : IGetPromptByChatIdWorkflow
{
    private IStorageService storageService;
    private IPromptContextBuilderFactory contextBuilderFactory;
    private IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
    private ILLMApiQueryPayloadBuilderFactory llmApiQueryPayloadBuilderFactory;

    public GetPromptByChatIdWorkflow(
        IStorageService storageService,
        IPromptContextBuilderFactory contextBuilderFactory,
        IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
        ILLMApiQueryPayloadBuilderFactory llmApiQueryPayloadBuilderFactory)
    {
        this.storageService = storageService;
        this.contextBuilderFactory = contextBuilderFactory;
        this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
        this.llmApiQueryPayloadBuilderFactory = llmApiQueryPayloadBuilderFactory;
    }

    public async Task<IWebApiResponseDto> GeneratePromptForChatId(string chatId, string tag)
    {
        if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(tag))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Prompt for chatId [{chatId}] and tag [{tag}] can't be found. Payload format is invalid."
            };
        }

        // Generate a context builder appropriate for the tag
        if (!Enum.TryParse(tag, out BackgroundQuerySystemTags outTag))
        {
            outTag = BackgroundQuerySystemTags.main;
        }

        var hotmessages = await storageService.GetAllHotMessagesAsync(chatId);
        var backgroundQuery = new BackgroundQueryDbModel
        {
            ChatId = chatId,
            Tags = new List<string> { outTag.ToString() },
            LinkedId = hotmessages?.Messages?.LastOrDefault()?.MessageId,
        };
        IPromptContextBuilder contextBuilder = await contextBuilderFactory.GenerateAsync(outTag, promptContextElementBuilderFactory, storageService, backgroundQuery);
        var promptContext = await contextBuilder.BuildAsync(backgroundQuery.ChatId);

        ILLMApiQueryPayloadBuilder llmApiQueryPayloadBuilder = llmApiQueryPayloadBuilderFactory.Create(LLMProviderType.OpenAICustom);

        LLMProviderConfig dummyConfig = new()
        {
            Model = "generated-prompt-dummy-model",
            Stream = true,
        };

        string payload = llmApiQueryPayloadBuilder.BuildPayload(promptContext, dummyConfig);
        return new PromptResponseDto()
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Prompt = llmApiQueryPayloadBuilder.TryGetPayloadAsSimpleString(payload),
        };
    }
}
