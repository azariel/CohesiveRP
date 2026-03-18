using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class DeleteLorebookWorkflow : IDeleteLorebookWorkflow
{
    private IStorageService storageService;

    public DeleteLorebookWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> DeleteLorebookAsync(DeleteLorebookRequestDto requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto?.LorebookId))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Request was incorrectly formatted."
            };
        }

        LorebookDbModel lorebook = await storageService.GetLorebookByIdAsync(requestDto.LorebookId);
        if (lorebook == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Lorebook with id {requestDto?.LorebookId} was not found."
            };
        }

        bool success = await storageService.DeleteLorebookAsync(lorebook);

        if (!success)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Lorebook with id {requestDto?.LorebookId} failed to delete."
            };
        }

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new LorebookResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };

        return responseDto;
    }
}
