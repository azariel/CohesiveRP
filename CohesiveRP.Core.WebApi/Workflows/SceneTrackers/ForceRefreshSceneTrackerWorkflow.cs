using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.SceneTrackers.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class ForceRefreshSceneTrackerWorkflow : IForceRefreshSceneTrackerWorkflow
{
    private IStorageService storageService;

    public ForceRefreshSceneTrackerWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> ForceRefreshSceneTracker(string chatId)
    {
        ChatDbModel chat = await storageService.GetChatAsync(chatId);
        if (chat == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Chat [{chatId}] to force refresh linked SceneTracker couldn't be found in storage."
            };
        }

        // TODO: Queue a new sceneTracker backgroundQuery
        var backgroundQueryModel = new CreateBackgroundQueryQueryModel
        {
            ChatId = chatId,
            Priority = BackgroundQueryPriority.Highest,// User is waiting!
            DependenciesTags = [BackgroundQuerySystemTags.sceneTracker.ToString()],// Can't run as long as another one with one of these tag is running or pending
            Tags = [BackgroundQuerySystemTags.main.ToString()],// This is a message from the player and thus is tagged as 'main'
        };

        var backgroundQuery = await storageService.AddBackgroundQueryAsync(backgroundQueryModel);

        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
