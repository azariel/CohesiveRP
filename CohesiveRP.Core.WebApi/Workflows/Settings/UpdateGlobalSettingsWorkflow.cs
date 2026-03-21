using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.ResponseDtos.Settings;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Settings;

namespace CohesiveRP.Core.WebApi.Workflows.Settings
{
    public class UpdateGlobalSettingsWorkflow : IUpdateGlobalSettingsWorkflow
    {
        private IStorageService storageService;

        public UpdateGlobalSettingsWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IWebApiResponseDto> UpdateGlobalSettings(UpdateGlobalSettingsRequestDto requestDto)
        {
            if (requestDto?.LLMProviders == null || requestDto.ChatCompletionPresetsMap == null || requestDto.Summary == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                    Message = $"Request was incorrectly formatted."
                };
            }

            var globalSettings = await storageService.GetGlobalSettingsAsync();
            if (globalSettings == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.NotFound,
                    Message = $"GlobalSettings were not found."
                };
            }

            GlobalSettingsDbModel dbModel = new()
            {
                Summary = requestDto.Summary,
                ChatCompletionPresetsMap = requestDto.ChatCompletionPresetsMap,
                LLMProviders = requestDto.LLMProviders,
                // Other fields are not overridable
            };

            bool success = await storageService.UpdateGlobalSettingsAsync(dbModel);

            if (!success)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"GlobalSettings update failed."
                };
            }

            var responseDto = new GlobalSettingsResponseDto
            {
                LLMProviders = globalSettings.LLMProviders,
                ChatCompletionPresetsMap = globalSettings.ChatCompletionPresetsMap,
                Summary = globalSettings.Summary,
            };

            responseDto.HttpResultCode = System.Net.HttpStatusCode.OK;
            return responseDto;


        }
    }
}
