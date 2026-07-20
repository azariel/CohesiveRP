using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries;
using CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.IllustrationQueries.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.IllustrationQueries
{
    public class GetIllustrationQueriesWorkflow : IGetIllustrationQueriesWorkflow
    {
        private IStorageService storageService;

        public GetIllustrationQueriesWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IWebApiResponseDto> GetQueries(string chatId)
        {
            var queries = await storageService.GetIllustrationQueriesAsync(s => s.ChatId == chatId);

            return new IllustrationQueriesResponseDto()
            {
                HttpResultCode = System.Net.HttpStatusCode.OK,
                Queries = queries.Select(q => new IllustrationQueryResponse()
                {
                    Id = q.IllustrationQueryId,
                    ChatId = q.ChatId,
                    Type = q.Type,
                    Status = q.Status,
                    CreatedAtUtc = q.CreatedAtUtc,
                }).ToArray()
            };
        }
    }
}
