using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Utils.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Characters;

public class DeleteCharacterAvatarWorkflow : IDeleteCharacterAvatarWorkflow
{
    private IStorageService storageService;

    public DeleteCharacterAvatarWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> DeleteCharacterAvatarAsync(string characterId, string avatarFileName)
    {
        ArgumentNullException.ThrowIfNull(avatarFileName);

        CharacterDbModel characterDbModel = await storageService.GetCharacterByIdAsync(characterId);
        if (characterDbModel == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Character [{characterId}] linked to the avatar to delete couldn't be found in storage."
            };
        }

        bool result = CharacterAvatarsUtils.DeleteCharacterAvatar(characterDbModel.Name, avatarFileName);
        if (!result)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Character [{characterId}] avatar [{avatarFileName}] deletion failed."
            };
        }

        // Reset main avatar to select one that still exists
        CharacterAvatarsUtils.RefreshDefaultAvatars($"{WebConstants.CharactersAvatarFilePath}\\{characterDbModel.Name.ToLowerInvariant()}", true);

        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
