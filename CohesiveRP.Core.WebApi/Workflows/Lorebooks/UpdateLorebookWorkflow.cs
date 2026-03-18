using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class UpdateLorebookWorkflow : IUpdateLorebookWorkflow
{
    private IStorageService storageService;

    public UpdateLorebookWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> UpdateLorebookAsync(UpdateLorebookRequestDto requestDto)
    {
        if (requestDto?.Lorebook == null || string.IsNullOrWhiteSpace(requestDto.LorebookId))
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

        LorebookDbModel dbModel = new()
        {
            LorebookId = requestDto.LorebookId,
            Name = requestDto.Lorebook.Name,
            Entries = requestDto.Lorebook.Entries,
            // Other fields are not overridable
        };
        bool success = await storageService.UpdateLorebookAsync(dbModel);

        if (!success)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Lorebook with id {requestDto?.LorebookId} failed to update."
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
