using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.BackgroundQueries;
using CohesiveRP.Core.WebApi.ResponseDtos.BackgroundQueries.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetBackgroundQueriesByChatIdWorkflow : IGetBackgroundQueriesByChatIdWorkflow
{
    private IStorageService storageService;

    public GetBackgroundQueriesByChatIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetBackgroundQueries(string chatId)
    {
        BackgroundQueryDbModel[] backgroundQueries = await storageService.GetBackgroundQueriesByChatIdAsync(chatId) ?? [];

        var responseDto = new BackgroundQueriesResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            ChatId = chatId,
            Queries = backgroundQueries.Select(s => new BackgroundQueryModel
            {
                LinkedId = s.LinkedId,
                BackgroundQueryId = s.BackgroundQueryId,
                Tags = s.Tags,
                DependenciesTags = s.DependenciesTags,
                Status = s.Status,
                Priority = s.Priority,
                Content = s.Content,
            }).ToArray(),
        };

        return responseDto;
    }
}
