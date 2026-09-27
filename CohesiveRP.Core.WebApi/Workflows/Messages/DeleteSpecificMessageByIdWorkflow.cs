using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class DeleteSpecificMessageByIdWorkflow : IDeleteSpecificMessageByIdWorkflow
{
    private IStorageService storageService;

    public DeleteSpecificMessageByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> DeleteSpecificMessage(GetSpecificMessageRequestDto requestDto)
    {
        bool success = await storageService.DeleteSpecificMessageAsync(requestDto.ChatId, requestDto.MessageId);

        if (!success)
        {
            LoggingManager.LogToFile("af1fbce7-d5d9-4247-aebe-021e161961b6", $"Couldn't delete message from id [{requestDto.MessageId}] in chat [{requestDto.ChatId}].");
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Message delete failed. Failed to delete the message in storage."
            };
        }

        // Delete the dependent objs since they don't apply anymore
        // rolls
        var chatCharacterRolls = await storageService.GetChatCharactersRollsByChatIdAsync(requestDto.ChatId);
        chatCharacterRolls.ChatCharactersRolls.Clear();
        chatCharacterRolls.CharacterNamesInScene.Clear();
        chatCharacterRolls.PlayerDescription = string.Empty;
        await storageService.UpdateChatCharactersRollsAsync(chatCharacterRolls);

        // sceneTracker
        var sceneTracker = await storageService.GetSceneTrackerAsync(requestDto.ChatId);
        sceneTracker.Content = sceneTracker.PreviousContent;
        sceneTracker.PreviousContent = string.Empty;
        await storageService.CreateOrUpdateSceneTrackerAsync(sceneTracker, false);

        return new DeleteMessageResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };
    }
}
