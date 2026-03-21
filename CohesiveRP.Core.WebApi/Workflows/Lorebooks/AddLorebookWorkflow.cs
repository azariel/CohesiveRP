using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class AddLorebookWorkflow : IAddLorebookWorkflow
{
    private IStorageService storageService;

    public AddLorebookWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> AddLorebookAsync(AddNewLorebookRequestDto requestDto)
    {
        var lorebook = await storageService.AddEmptyLorebookAsync();

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
