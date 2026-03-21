using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions;
using CohesiveRP.Storage.QueryModels.Personas;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class AddPersonaWorkflow : IAddPersonaWorkflow
{
    private IStorageService storageService;

    public AddPersonaWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> AddPersonaAsync(AddNewPersonaRequestDto requestDto)
    {
        var queryModel = new AddPersonaQueryModel
        {
            Name = "New Persona",
            Description = "Description of your new persona."
        };

        var persona = await storageService.AddPersonaAsync(queryModel);

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
