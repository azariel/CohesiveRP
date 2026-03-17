using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.BackgroundQueries;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetBackgroundQueryWorkflow : IGetBackgroundQueryWorkflow
{
    private IStorageService storageService;

    public GetBackgroundQueryWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetBackgroundQuery(string queryId)
    {
        BackgroundQueryDbModel backgroundQuery = await storageService.GetBackgroundQueryAsync(queryId);
        backgroundQuery ??= new BackgroundQueryDbModel();

        var responseDto = new BackgroundQueryResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            ChatId = backgroundQuery.ChatId,
            LinkedId = backgroundQuery.LinkedId,
            BackgroundQueryId = backgroundQuery.BackgroundQueryId,
            Tags = backgroundQuery.Tags,
            DependenciesTags = backgroundQuery.DependenciesTags,
            Status = backgroundQuery.Status,
            Priority = backgroundQuery.Priority,
            Content = backgroundQuery.Content,
        };

        return responseDto;
    }
}
