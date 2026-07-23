using System.Text;
using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets;

public class RegenerateCharacterSheetWorkflow : IRegenerateCharacterSheetWorkflow
{
    private IStorageService storageService;
    private IHttpLLMApiProviderService httpLLMApiProviderService;

    public RegenerateCharacterSheetWorkflow(IStorageService storageService,
        IHttpLLMApiProviderService httpLLMApiProviderService)
    {
        this.storageService = storageService;
        this.httpLLMApiProviderService = httpLLMApiProviderService;
    }

    public async Task<IWebApiResponseDto> RegenerateCharacterSheetAsync(RegenerateCharacterSheetRequestDto requestDto)
    {
        if (requestDto?.CharacterId == null && requestDto.PersonaId == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Request was incorrectly formatted. CharacterId or PersonaId is missing."
            };
        }

        var characterSheets = await storageService.GetCharacterSheetsByFuncAsync(f => (!string.IsNullOrWhiteSpace(requestDto.CharacterId) && f.CharacterId == requestDto.CharacterId) || (!string.IsNullOrWhiteSpace(requestDto.PersonaId) && f.PersonaId == requestDto.PersonaId));
        var characterSheetDbModel = characterSheets.FirstOrDefault();
        if (characterSheetDbModel?.CharacterSheet == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"CharacterSheet to regenerate couldn't be found."
            };
        }

        var characterSheet = characterSheetDbModel.CharacterSheet;

        StringBuilder str = new();

        var preset = DynamicCharacterSheetCreatorCompletionPresetInjector.InjectPreset();

        if (preset?.Format?.OrderedElementsWithinTheGlobalPromptContext == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Completion preset to generate character sheet was not found."
            };
        }

        foreach (var item in preset.Format.OrderedElementsWithinTheGlobalPromptContext.Where(w => w.Enabled && w.Options?.Format != null && w.Tag == PromptContextFormatTag.Directive))
        {
            str.AppendLine(item.Options.Format);
        }

        str.AppendLine("<information_about_character_to_analyze>");

        string description = null;
        if (!string.IsNullOrWhiteSpace(requestDto.CharacterId))
        {
            var character = await storageService.GetCharacterByIdAsync(requestDto.CharacterId);
            description = character?.Description;
        } else if (!string.IsNullOrWhiteSpace(requestDto.PersonaId))
        {
            var persona = await storageService.GetPersonaByIdAsync(requestDto.PersonaId);
            description = persona?.Description;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            str.AppendLine($"  {description}");
        }

        str.AppendLine("</information_about_character_to_analyze>");

        var promptContext = new CharacterSheetRegenerationPromptContext
        {
            Value = null,
            Messages =
                [
                    new OpenAIChatCompletionMessage
                    {
                        Role = OpenAIChatCompletionRole.system,
                        Content = str.ToString(),
                    }
                ],
            ShareableContextLinks = null
        };

        GlobalSettingsDbModel globalSettings = await storageService.GetGlobalSettingsAsync();
        LLMProviderConfig[] availableLLMApiProviders = globalSettings.LLMProviders.Where(w => w.Tags.Contains(ChatCompletionPresetType.SPECIAL_CharacterSheetGeneration)).ToArray();

        CancellationToken token = new CancellationTokenSource(900000).Token;
        IHttpLLMApiQueryResponseDto response = await httpLLMApiProviderService.QueryApiAsync(ChatCompletionPresetType.SPECIAL_CharacterSheetGeneration.ToString(), globalSettings.LLMProviders.ToArray(), availableLLMApiProviders, promptContext, null, token);

        if (response?.Messages == null || response.Messages.Length <= 0)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"CharacterSheet to regenerate request to LLM failed."
            };
        }

        // try to deserialize response
        CharacterSheet responseJson = null;
        try
        {
            string rawResponse = response.Messages.First().Content;

            // First, try to parse the thinking and reasoning from the LLM response
            string message = ChatMessageParserUtils.ParseMessage(rawResponse);


            responseJson = LLMResponseParser.ParseFromApiMessageContent<CharacterSheet>(message);
        } catch (Exception e)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"CharacterSheet to regenerate request to LLM failed due LLM invalid response. {e.Message}"
            };
        }

        // Update the characterSheet
        characterSheetDbModel.CharacterSheet = responseJson;
        var updateResult = await storageService.UpdateCharacterSheetAsync(characterSheetDbModel);
        if (!updateResult)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Couldn't update regenerated characterSheet in storage."
            };
        }

        var responseDto = new GetCharacterSheetResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            CharacterId = characterSheetDbModel.CharacterId,
            PersonaId = characterSheetDbModel.PersonaId,
            CharacterSheetId = characterSheetDbModel.CharacterSheetId,
            LastActivityAtUtc = characterSheetDbModel.LastActivityAtUtc,
            CharacterSheet = characterSheetDbModel.CharacterSheet,
        };

        return responseDto;
    }
}
