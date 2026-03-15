using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.SceneTrackers.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetSceneTrackerByIdWorkflow : IGetSceneTrackerByIdWorkflow
{
    private IStorageService storageService;

    public GetSceneTrackerByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetSceneTrackerByChatId(string chatId)
    {
        SceneTrackerDbModel scenetracker = await storageService.GetSceneTrackerAsync(chatId);

        if (scenetracker == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"SceneTracker with chat id {chatId} was not found."
            };
        }

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new SceneTrackerResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            SceneTracker = new SceneTrackerResponse
            {
                ChatId=  scenetracker.ChatId,
                Content = scenetracker.Content,
                SceneTrackerId = scenetracker.SceneTrackerId,
                CreatedAtUtc = scenetracker.CreatedAtUtc,
                LinkMessageId = scenetracker.LinkMessageId,
            }
        };

        return responseDto;
    }
}
