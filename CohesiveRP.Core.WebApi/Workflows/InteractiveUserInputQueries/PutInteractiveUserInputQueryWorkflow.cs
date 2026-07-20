using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.InteractiveUserInputQueries;
using CohesiveRP.Core.WebApi.ResponseDtos.InteractiveUserInputQueries;
using CohesiveRP.Core.WebApi.ResponseDtos.InteractiveUserInputQueries.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.InteractiveUserInputQueries.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.InteractiveUserInputQueries
{
    public class PutInteractiveUserInputQueryWorkflow : IPutInteractiveUserInputQueryWorkflow
    {
        private IStorageService storageService;

        public PutInteractiveUserInputQueryWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IWebApiResponseDto> PutInteractiveUserInputQueryAsync(PutInteractiveUserInputQueryRequestDto requestDto)
        {
            if (string.IsNullOrWhiteSpace(requestDto?.QueryId))
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                    Message = $"InteractiveUserInputQuery creation/update failed. The specified queryId was null or empty."
                };
            }

            var queries = await storageService.GetInteractiveUserInputQueriesAsync(s => s.InteractiveUserInputQueryId == requestDto.QueryId);
            var query = queries.FirstOrDefault();

            if(query == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.NotFound,
                    Message = $"InteractiveUserInputQuery with queryId ['{requestDto.QueryId}'] not found."
                };
            }

            query.Metadata = requestDto.Query.Metadata ?? query.Metadata;
            query.UserChoice = requestDto.Query.UserChoice;
            query.Status = requestDto.Query.Status;

            var result = await storageService.UpdateInteractiveUserInputQueryAsync(query);

            return new InteractiveUserInputQueriesResponseDto()
            {
                HttpResultCode = System.Net.HttpStatusCode.OK,
                Queries = [new InteractiveUserInputQueryResponse()
                {
                    Id = query.InteractiveUserInputQueryId,
                    CreatedAtUtc = query.CreatedAtUtc,
                    ChatId = query.ChatId,
                    Status = query.Status,
                    Type = query.Type,
                    SceneTrackerId = query.SceneTrackerId,
                    Metadata = query.Metadata,
                }]
            };
        }
    }
}
