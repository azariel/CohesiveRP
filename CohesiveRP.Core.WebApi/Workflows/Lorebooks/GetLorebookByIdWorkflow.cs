using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetLorebookByIdWorkflow : IGetLorebookByIdWorkflow
{
    private IStorageService storageService;

    public GetLorebookByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetLorebookByIdAsync(string lorebookId)
    {
        LorebookDbModel lorebook = await storageService.GetLorebookByIdAsync(lorebookId);

        if(lorebook == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Lorebook with id {lorebookId} was not found."
            };
        }

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new LorebookResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Lorebook = new LorebookResponse
            {
                LorebookId = lorebook.LorebookId,
                Name = lorebook.Name,
                LastActivityAtUtc = lorebook.LastActivityAtUtc,
                Entries = lorebook.Entries,
            }
        };

        return responseDto;
    }
}
