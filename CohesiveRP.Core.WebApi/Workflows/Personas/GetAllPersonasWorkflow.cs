using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetAllPersonasWorkflow : IGetAllPersonasWorkflow
{
    private IStorageService storageService;

    public GetAllPersonasWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetAllPersonasAsync()
    {
        PersonaDbModel[] personas = await storageService.GetPersonasAsync();

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new PersonasResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            // TODO: pagination instead of take(50)
            Personas = personas.Take(50).Select(s => new PersonaResponse
            {
                PersonaId = s.PersonaId,
                Name = s.Name,
                IsDefault = s.IsDefault,
                Description = s.Description,
            }).OrderByDescending(o => o.LastActivityAtUtc).ToList()
        };

        return responseDto;
    }
}
