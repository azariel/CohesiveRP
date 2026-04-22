using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Utils.Characters;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class UpdateCharacterWorkflow : IUpdateCharacterWorkflow
{
    private IStorageService storageService;

    public UpdateCharacterWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> UpdateCharacterAsync(UpdateCharacterRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentNullException.ThrowIfNull(requestDto.CharacterId);
        ArgumentNullException.ThrowIfNull(requestDto.CharacterName);
        ArgumentNullException.ThrowIfNull(requestDto.CharacterDescription);
        ArgumentNullException.ThrowIfNull(requestDto.Tags);

        CharacterDbModel currentCharacter = await storageService.GetCharacterByIdAsync(requestDto.CharacterId);
        if (currentCharacter == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Character [{requestDto.CharacterId}] to update couldn't be found in storage."
            };
        }

        currentCharacter.Name = requestDto.CharacterName;
        currentCharacter.Creator = requestDto.Creator;
        currentCharacter.CreatorNotes = requestDto.CreatorNotes;
        currentCharacter.FirstMessage = requestDto.FirstMessage;
        currentCharacter.Description = requestDto.CharacterDescription;
        currentCharacter.Tags = requestDto.Tags?.ToList();
        currentCharacter.AlternateGreetings = requestDto.AlternateGreetings?.ToList();
        currentCharacter.ImageGenerationConfiguration = requestDto.ImageGenerationConfiguration;

        bool result = await storageService.UpdateCharacterAsync(currentCharacter);
        if (!result)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Character [{requestDto.CharacterId}] update failed."
            };
        }

        CharacterUtils.CreateCharacterAssets(currentCharacter);

        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
