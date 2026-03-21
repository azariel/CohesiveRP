using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetAllLorebooksWorkflow : IGetAllLorebooksWorkflow
{
    private IStorageService storageService;

    public GetAllLorebooksWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetAllLorebooksAsync()
    {
        LorebookDbModel[] lorebooks = await storageService.GetLorebooksAsync();

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new LorebooksResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            // TODO: pagination instead of take(50)
            Lorebooks = lorebooks?.Take(50).Select(s => new LorebookResponse
            {
                LorebookId = s.LorebookId,
                Name = s.Name,
                LastActivityAtUtc = s.LastActivityAtUtc,
                //Entries = s.Entries,
            }).OrderByDescending(o => o.LastActivityAtUtc).ToList()
        };

        return responseDto;
    }
}
