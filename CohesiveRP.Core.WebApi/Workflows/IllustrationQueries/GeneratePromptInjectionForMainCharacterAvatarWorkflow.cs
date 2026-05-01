using System.Text;
using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;
using CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries.BusinessObjects;
using CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries;
using CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets;
using CohesiveRP.Core.WebApi.Workflows.IllustrationQueries.Abstractions;
using CohesiveRP.Storage;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.IllustrationQueries
{
    public class GeneratePromptInjectionForMainCharacterAvatarWorkflow : IGeneratePromptInjectionForMainCharacterAvatarWorkflow
    {
        private IStorageService storageService;
        private IHttpLLMApiProviderService httpLLMApiProviderService;

        public GeneratePromptInjectionForMainCharacterAvatarWorkflow(IStorageService storageService, IHttpLLMApiProviderService httpLLMApiProviderService)
        {
            this.storageService = storageService;
            this.httpLLMApiProviderService = httpLLMApiProviderService;
        }

        public async Task<IWebApiResponseDto> Generate(GeneratePromptInjectionForCharacterIllustrationRequestDto requestDto)
        {
            // Validate request
            if (string.IsNullOrEmpty(requestDto?.CharacterId))
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                    Message = $"Prompt injection generation failed. The specified payload was null or empty."
                };
            }

            // Get character details
            var character = await storageService.GetCharacterByIdAsync(requestDto.CharacterId);
            if (character == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.NotFound,
                    Message = $"Character not found."
                };
            }

            // Get characterSheet
            var characterSheet = await storageService.GetCharacterSheetByCharacterIdAsync(requestDto.CharacterId);
            if (characterSheet == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.NotFound,
                    Message = $"CharacterSheet not found."
                };
            }

            //// Queue a background query to generate the prompt injection based on character details and outfit
            //var backgroundQuery =await storageService.AddBackgroundQueryAsync(new CreateBackgroundQueryQueryModel
            //{
            //    ChatId = null, // No specific chat context for this background query
            //    LinkedId = requestDto.CharacterId,
            //    Tags = [BackgroundQuerySystemTags.illustrationPromptInjectionForCharacterAvatar.ToString()],
            //    Priority = BackgroundQueryPriority.Highest,
            //});

            //// Wait for the background query to complete and get the generated prompt injection result
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //while (true)
            //{
            //    var backgroundQueryUpdated = await storageService.GetBackgroundQueryAsync(backgroundQuery.BackgroundQueryId);
            //    if (backgroundQueryUpdated == null || backgroundQueryUpdated.Status == BackgroundQueryStatus.Completed)
            //    {
            //        break;
            //    }

            //    await Task.Delay(1000); // Wait for 1 second before checking again
            //    if (stopwatch.Elapsed.TotalSeconds > 900) // Timeout after 15 minutes
            //    {
            //        return new WebApiException
            //        {
            //            HttpResultCode = System.Net.HttpStatusCode.RequestTimeout,
            //            Message = $"Prompt injection generation timed out."
            //        };
            //    }
            //}

            //var backgroundQueryFinal = await storageService.GetBackgroundQueryAsync(backgroundQuery.BackgroundQueryId);

            var completionChatsPreset = await storageService.GetChatCompletionPresetAsync(StorageConstants.DEFAULT_ILLUSTRATION_PROMPT_INJECTION_FOR_CHARACTER_AVATAR_COMPLETION_PRESET);
            if (completionChatsPreset == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"Chat completion preset [{StorageConstants.DEFAULT_ILLUSTRATION_PROMPT_INJECTION_FOR_CHARACTER_AVATAR_COMPLETION_PRESET}] not found."
                };
            }

            StringBuilder str = new();
            foreach (var presetElement in completionChatsPreset.Format.OrderedElementsWithinTheGlobalPromptContext.Where(w => w.Enabled))
            {
                string value = presetElement.Options.Format
                    .Replace("{{character_name}}", character.Name)
                    .Replace("{{character_race}}", characterSheet.CharacterSheet.Race)
                    .Replace("{{character_bodyType}}", characterSheet.CharacterSheet.BodyType)
                    .Replace("{{character_height}}", characterSheet.CharacterSheet.Height)
                    .Replace("{{character_eyeColor}}", characterSheet.CharacterSheet.EyeColor)
                    .Replace("{{character_skinColor}}", characterSheet.CharacterSheet.SkinColor)
                    .Replace("{{character_hairColor}}", characterSheet.CharacterSheet.HairColor)
                    .Replace("{{character_hairStyle}}", characterSheet.CharacterSheet.HairStyle)
                    .Replace("{{character_earShape}}", characterSheet.CharacterSheet.EarShape)
                    .Replace("{{character_clothesPreferences}}", characterSheet.CharacterSheet.ClothesPreference);
                str.AppendLine(value);
            }

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
            LLMProviderConfig[] availableLLMApiProviders = globalSettings.LLMProviders.Where(w => w.Tags.Contains(ChatCompletionPresetType.IllustrationPromptInjectionForCharacterAvatar)).ToArray();

            CancellationToken token = new CancellationTokenSource(900000).Token;
            IHttpLLMApiQueryResponseDto response = await httpLLMApiProviderService.QueryApiAsync(ChatCompletionPresetType.IllustrationPromptInjectionForCharacterAvatar.ToString(), globalSettings.LLMProviders.ToArray(), availableLLMApiProviders, promptContext, null, token);

            if (response?.Messages == null || response.Messages.Length <= 0)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"InjectionPrompt request to LLM failed."
                };
            }

            string promptInjectionResult = LLMResponseParser.ParseOnlyJson(response.Messages.First().Content);
            StringResultWrapper promptInjectionResultWrapper = JsonCommonSerializer.DeserializeFromString<StringResultWrapper>(promptInjectionResult);

            if (string.IsNullOrWhiteSpace(promptInjectionResultWrapper?.Content))
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"InjectionPrompt request to LLM failed to generate satisfying response."
                };
            }

            return new GeneratePromptInjectionForMainCharacterAvatarResponseDto()
            {
                HttpResultCode = System.Net.HttpStatusCode.OK,
                PromptInjection = promptInjectionResultWrapper.Content,
            };
        }
    }
}
