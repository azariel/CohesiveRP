using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.InteractiveUserInputQueries;
using CohesiveRP.Core.WebApi.ResponseDtos.InteractiveUserInputQueries.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.InteractiveUserInputQueries.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.InteractiveUserInputQueries
{
    public class GetInteractiveUserInputQueriesFromChatIdWorkflow : IGetInteractiveUserInputQueriesFromChatIdWorkflow
    {
        private IStorageService storageService;

        public GetInteractiveUserInputQueriesFromChatIdWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IWebApiResponseDto> GetInteractiveUserInputQueryAsync(string chatId)
        {
            var queries = await storageService.GetInteractiveUserInputQueriesAsync(s => s.ChatId == chatId);

            return new InteractiveUserInputQueriesResponseDto()
            {
                HttpResultCode = System.Net.HttpStatusCode.OK,
                Queries = queries.Select(q => new InteractiveUserInputQueryResponse()
                {
                    Id = q.InteractiveUserInputQueryId,
                    CreatedAtUtc = q.CreatedAtUtc,
                    ChatId = q.ChatId,
                    Status = q.Status,
                    Type = q.Type,
                    SceneTrackerId = q.SceneTrackerId,
                    Metadata = q.Metadata,
                }).ToArray()
            };
        }
    }
}
