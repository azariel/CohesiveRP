using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters.CharacterSheets;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets;

public class AddCharacterSheetWorkflow : IAddCharacterSheetWorkflow
{
    private IStorageService storageService;

    public AddCharacterSheetWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> AddCharacterSheetAsync(AddCharacterSheetRequestDto requestDto)
    {
        if (requestDto?.CharacterSheet == null || (string.IsNullOrWhiteSpace(requestDto.CharacterId) && string.IsNullOrWhiteSpace(requestDto.PersonaId)))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Request to add a new characterSheet was malformed or CharacterId was missing."
            };
        }

        CharacterSheetDbModel existingCharacterSheet = null;
        if (string.IsNullOrWhiteSpace(requestDto.CharacterId))
        {
            var elements = await storageService.GetCharacterSheetsByFuncAsync(f=>f.PersonaId == requestDto.PersonaId);
            existingCharacterSheet = elements?.FirstOrDefault();
        } else
        {
            existingCharacterSheet = await storageService.GetCharacterSheetByCharacterIdAsync(requestDto.CharacterId);
        }

        if (existingCharacterSheet != null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Character with Id [{requestDto.CharacterId}] already has a characterSheet."
            };
        }

        var dbModel = new CharacterSheetDbModel
        {
            CharacterId = requestDto.CharacterId,
            PersonaId = requestDto.PersonaId,
            CharacterSheet = requestDto.CharacterSheet,
        };

        CharacterSheetDbModel result = await storageService.AddCharacterSheetAsync(dbModel);

        if (result == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Request to add a new characterSheet failed to process against the storage."
            };
        }

        var responseDto = new CharacterSheetResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            CharacterSheet = result.CharacterSheet,
        };

        return responseDto;
    }
}
