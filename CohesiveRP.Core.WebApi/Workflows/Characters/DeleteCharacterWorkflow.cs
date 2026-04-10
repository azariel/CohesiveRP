using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class DeleteCharacterWorkflow : IDeleteCharacterWorkflow
{
    private IStorageService storageService;

    public DeleteCharacterWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> DeleteCharacterAsync(string characterId)
    {
        ArgumentNullException.ThrowIfNull(characterId);

        CharacterDbModel currentCharacter = await storageService.GetCharacterByIdAsync(characterId);
        if (currentCharacter == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Character [{characterId}] to delete couldn't be found in storage."
            };
        }

        bool result = await storageService.DeleteCharacterAsync(currentCharacter);
        if (!result)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Character [{characterId}] deletion failed."
            };
        }

        // delete character folder
        string directoryCharacter = Path.Combine(WebConstants.CharactersAvatarFilePath, currentCharacter.Name.ToLowerInvariant());
        if (Directory.Exists(directoryCharacter))
        {
            Directory.Delete(directoryCharacter, true);
        }

        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
