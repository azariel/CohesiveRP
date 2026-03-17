using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class DeletePersonaWorkflow : IDeletePersonaWorkflow
{
    private IStorageService storageService;

    public DeletePersonaWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> DeletePersonaAsync(DeletePersonaRequestDto requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto?.PersonaId))
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

        bool success = await storageService.DeletePersonaAsync(persona);

        if (!success)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Persona with id {requestDto?.PersonaId} failed to delete."
            };
        }

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new PersonaResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
