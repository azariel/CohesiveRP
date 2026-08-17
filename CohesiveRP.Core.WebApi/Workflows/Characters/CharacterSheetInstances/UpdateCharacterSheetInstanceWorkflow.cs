using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.RequestDtos.Characters.CharacterSheetInstances;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets;

public class UpdateCharacterSheetInstanceWorkflow : IUpdateCharacterSheetInstanceWorkflow
{
    private IStorageService storageService;

    public UpdateCharacterSheetInstanceWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> UpdateCharacterSheetInstanceAsync(UpdateCharacterSheetInstanceRequestDto requestDto)
    {
        if (requestDto?.CharacterSheet == null || string.IsNullOrWhiteSpace(requestDto.CharacterSheetInstanceId) || string.IsNullOrWhiteSpace(requestDto.CharacterId))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Request to update a characterSheetInstance was malformed or CharacterId was missing."
            };
        }

        if (string.IsNullOrWhiteSpace(requestDto.CharacterId))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Request to update a characterSheetInstance CharacterId was missing."
            };
        }

        var existingCharacterSheetInstances = await storageService.GetCharacterSheetInstancesByFuncAsync(f => f.ChatId == requestDto.ChatId);
        if (existingCharacterSheetInstances == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Characters tied to chatId [{requestDto.ChatId}] were not found in storage."
            };
        }

        var existingCharacterSheetInstance = existingCharacterSheetInstances.FirstOrDefault();
        var existingCharacterSheetInstanceToUpdate = existingCharacterSheetInstance?.CharacterSheetInstances?.FirstOrDefault(f => f.CharacterId == requestDto.CharacterId && f.CharacterSheetInstanceId == requestDto.CharacterSheetInstanceId);
        if (existingCharacterSheetInstance == null || existingCharacterSheetInstanceToUpdate == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Character with Id [{requestDto.CharacterId}] did not have a characterSheetInstance to update."
            };
        }

        existingCharacterSheetInstanceToUpdate.CharacterSheet = requestDto.CharacterSheet;
        bool result = await storageService.UpdateCharacterSheetsInstanceAsync(existingCharacterSheetInstance);
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
            CharacterId = existingCharacterSheetInstanceToUpdate.CharacterId,
            PersonaId = existingCharacterSheetInstanceToUpdate.PersonaId,
            CharacterSheetId = existingCharacterSheetInstanceToUpdate.CharacterSheetId,
            CharacterSheet = existingCharacterSheetInstanceToUpdate.CharacterSheet,
        };

        return responseDto;
    }
}
