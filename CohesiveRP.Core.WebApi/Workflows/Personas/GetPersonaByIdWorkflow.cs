using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetPersonaByIdWorkflow : IGetPersonaByIdWorkflow
{
    private IStorageService storageService;

    public GetPersonaByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetPersonaByIdAsync(string personaId)
    {
        PersonaDbModel persona = await storageService.GetPersonaByIdAsync(personaId);

        if(persona == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Persona with id {personaId} was not found."
            };
        }

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new PersonaResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            // TODO: pagination instead of take(50)
            Persona = new PersonaResponse
            {
                PersonaId = persona.PersonaId,
                Name = persona.Name,
                Description = persona.Description,
                LastActivityAtUtc = persona.LastActivityAtUtc,
            }
        };

        return responseDto;
    }
}
