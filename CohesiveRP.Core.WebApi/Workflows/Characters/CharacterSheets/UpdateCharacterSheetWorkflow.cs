using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets;

public class UpdateCharacterSheetWorkflow : IUpdateCharacterSheetWorkflow
{
    private IStorageService storageService;

    public UpdateCharacterSheetWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> UpdateCharacterSheetAsync(UpdateCharacterSheetRequestDto requestDto)
    {
        if (requestDto?.CharacterSheet == null || string.IsNullOrWhiteSpace(requestDto.CharacterSheetId) || (string.IsNullOrWhiteSpace(requestDto.CharacterId) && string.IsNullOrWhiteSpace(requestDto.PersonaId)))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Request to update a characterSheet was malformed or CharacterId was missing."
            };
        }

        CharacterSheetDbModel existingCharacterSheet = null;
        if (string.IsNullOrWhiteSpace(requestDto.CharacterId))
        {
            var elements = await storageService.GetCharacterSheetsByFuncAsync(f => f.PersonaId == requestDto.PersonaId);
            existingCharacterSheet = elements?.FirstOrDefault();
        } else
        {
            existingCharacterSheet = await storageService.GetCharacterSheetByCharacterIdAsync(requestDto.CharacterId);
        }
        if (existingCharacterSheet == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Character with Id [{requestDto.CharacterId}] did not have a characterSheet to update."
            };
        }

        var dbModel = new CharacterSheetDbModel
        {
            CharacterSheetId = requestDto.CharacterSheetId,
            CharacterId = requestDto.CharacterId,
            PersonaId = requestDto.PersonaId,
            CharacterSheet = requestDto.CharacterSheet,
        };

        bool result = await storageService.UpdateCharacterSheetAsync(dbModel);
        if (!result)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Request to update a characterSheet failed to process against the storage."
            };
        }

        var responseDto = new GetCharacterSheetResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            CharacterId = dbModel.CharacterId,
            PersonaId = dbModel.PersonaId,
            LastActivityAtUtc = dbModel.LastActivityAtUtc,
            CharacterSheetId = dbModel.CharacterSheetId,
            CharacterSheet = dbModel.CharacterSheet,
        };

        return responseDto;
    }
}
