using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos;
using CohesiveRP.Core.WebApi.ResponseDtos.BackgroundQueries.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
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

    private static string FormatPropertyValue(object value)
    {
        return value switch
        {
            null => null,
            string s => string.IsNullOrWhiteSpace(s) ? null : s.Trim(),
            string[] arr => FormatStringArray(arr),
            DateTime dt => dt.ToString("yyyy-MM-dd"),
            Array arr when arr.Length > 0 => FormatComplexArray(arr),
            Array => null,
            Enum e => e.ToString(),
            _ => value.ToString()
        };
    }

    private static string FormatStringArray(string[] arr)
    {
        string[] nonEmpty = arr?.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray() ?? [];
        return nonEmpty.Length > 0
            ? string.Join(", ", nonEmpty)
            : null;
    }

    private static string FormatComplexArray(Array arr)
    {
        return JsonSerializer.Serialize(arr, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false
        });
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

        var characterSheets = await storageService.GetCharacterSheetsByFuncAsync(f => f.CharacterId == requestDto.CharacterId || f.PersonaId == requestDto.PersonaId);
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
        var properties = typeof(CharacterSheet).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            try
            {
                string tagName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;

                // Those particular properties will be handled manually for more control
                if (tagName == "pathfinderAttributes" || tagName == "pathfinderSkills")
                    continue;

                object value = property.GetValue(characterSheet);
                string formattedValue = FormatPropertyValue(value);

                if (string.IsNullOrWhiteSpace(formattedValue))
                    continue;

                str.AppendLine($"  <{tagName}>{formattedValue}</{tagName}>");
            } catch (Exception)
            {
                // ignore
            }
        }

        // Handle the Pathfinder special properties
        str.AppendLine($"    <Attributes>");
        foreach (PathfinderAttribute attribute in characterSheet.PathfinderAttributesValues)
        {
            str.AppendLine($"      <{attribute.AttributeType}>{attribute.Value}</{attribute.AttributeType}>");
        }

        str.AppendLine($"    </Attributes>");
        str.AppendLine($"    <Skills>");
        foreach (PathfinderSkillAttributes skill in characterSheet.PathfinderSkillsValues)
        {
            str.AppendLine($"      <{skill.SkillType}>{skill.Value}</{skill.SkillType}>");
        }
        str.AppendLine($"    </Skills>");

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
        IHttpLLMApiQueryResponseDto response = await httpLLMApiProviderService.QueryApiAsync(ChatCompletionPresetType.SPECIAL_CharacterSheetGeneration.ToString(), availableLLMApiProviders, promptContext);

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
            responseJson = LLMResponseParser.ParseOnlyJson<CharacterSheet>(response.Messages.First().Content);
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

        return new BasicResponseDto()
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };
    }
}
