using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class UpdatePersonaWorkflow : IUpdatePersonaWorkflow
{
    private IStorageService storageService;

    public UpdatePersonaWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> UpdatePersonaAsync(UpdatePersonaRequestDto requestDto)
    {
        if (requestDto?.Persona == null || string.IsNullOrWhiteSpace(requestDto.PersonaId))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Request was incorrectly formatted."
            };
        }

        PersonaDbModel persona = await storageService.GetPersonaByIdAsync(requestDto.PersonaId);
        if (persona == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Persona with id {requestDto?.PersonaId} was not found."
            };
        }

        PersonaDbModel dbModel = new()
        {
            PersonaId = requestDto.PersonaId,
            Description = requestDto.Persona.Description,
            Name = requestDto.Persona.Name,
            IsDefault = requestDto.Persona.IsDefault,
            // Other fields are not overridable
        };
        bool success = await storageService.UpdatePersonaAsync(dbModel);

        if (!success)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Persona with id {requestDto?.PersonaId} failed to update."
            };
        }

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new PersonaResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Persona = new PersonaResponse
            {
                PersonaId = persona.PersonaId,
                Name = persona.Name,
                IsDefault = persona.IsDefault,
                Description = persona.Description,
                LastActivityAtUtc = persona.LastActivityAtUtc,
            }
        };

        return responseDto;
    }
}
