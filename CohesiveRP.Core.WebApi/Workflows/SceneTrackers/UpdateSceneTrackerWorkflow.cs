using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.SceneTrackers.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.QueryModels.SceneTracker;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class UpdateSceneTrackerWorkflow : IUpdateSceneTrackerWorkflow
{
    private IStorageService storageService;

    public UpdateSceneTrackerWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> UpdateSceneTracker(UpdateSceneTrackerRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentNullException.ThrowIfNull(requestDto.SceneTracker?.Content);

        SceneTrackerDbModel sceneTracker = await storageService.GetSceneTrackerAsync(requestDto.ChatId);
        if (sceneTracker == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"SceneTracker linked to Chat [{requestDto.ChatId}] to update linked SceneTracker couldn't be found in storage."
            };
        }

        CreateSceneTrackerQueryModel queryModel = new CreateSceneTrackerQueryModel
        {
            ChatId = requestDto.ChatId,
            Content = requestDto.SceneTracker.Content,
        };

        var result = await storageService.CreateOrUpdateSceneTrackerAsync(queryModel);
        if (result == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"SceneTracker [{sceneTracker.SceneTrackerId}] update failed."
            };
        }

        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
