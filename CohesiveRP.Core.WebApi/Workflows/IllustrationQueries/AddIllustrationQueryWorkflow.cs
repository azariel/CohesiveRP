using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries;
using CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries;
using CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.IllustrationQueries.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries;

namespace CohesiveRP.Core.WebApi.Workflows.Chats
{
    public class AddIllustrationQueryWorkflow : IAddIllustrationQueryWorkflow
    {
        private IStorageService storageService;

        public AddIllustrationQueryWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IWebApiResponseDto> AddQuery(AddIllustrationQueryRequestDto requestDto)
        {
            if (requestDto?.Body == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                    Message = $"IllustrationQuery creation failed. The specified payload was null or empty."
                };
            }

            var newlyCreatedQuery = await storageService.AddIllustrationQueryAsync(new IllustrationQueryDbModel
            {
                ChatId = requestDto.Body.ChatId,
                Type = requestDto.Body.Type,
                CharacterId = requestDto.Body.CharacterId,
            });

            if (newlyCreatedQuery == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"IllustrationQuery creation failed in storage."
                };
            }

            return new IllustrationQueryResponseDto
            {
                HttpResultCode = System.Net.HttpStatusCode.OK,
                Query = new IllustrationQueryResponse
                {
                    Id = newlyCreatedQuery.IllustrationQueryId,
                    ChatId = newlyCreatedQuery.ChatId,
                    CreatedAtUtc = newlyCreatedQuery.CreatedAtUtc,
                    Type = newlyCreatedQuery.Type,
                    Status = newlyCreatedQuery.Status,
                }
            };
        }
    }
}
