using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Settings;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetGlobalSettingsWorkflow : IGetGlobalSettingsWorkflow
{
    private IStorageService storageService;

    public GetGlobalSettingsWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetGlobalSettings()
    {
        var globalSettings = await storageService.GetGlobalSettingsAsync();
        globalSettings ??= new GlobalSettingsDbModel();

        var responseDto = new GlobalSettingsResponseDto
        {
            LLMProviders = globalSettings.LLMProviders,
        };

        responseDto.HttpResultCode = System.Net.HttpStatusCode.OK;
        return responseDto;
    }
}
