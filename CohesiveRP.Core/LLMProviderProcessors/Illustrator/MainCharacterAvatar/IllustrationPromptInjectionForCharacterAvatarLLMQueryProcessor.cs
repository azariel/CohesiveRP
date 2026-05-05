using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.LLMProviderProcessors.DynamicCharacterCreator.BusinessObjects;
using CohesiveRP.Core.LLMProviderProcessors.Illustrator.MainCharacterAvatar.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Illustrator.MainCharacterAvatar;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.Illustrator.MainCharacterAvatar
{
    internal class IllustrationPromptInjectionForCharacterAvatarLLMQueryProcessor : LLMQueryProcessor
    {
        public IllustrationPromptInjectionForCharacterAvatarLLMQueryProcessor(
            ChatCompletionPresetType completionPresetType,
            BackgroundQuerySystemTags tag,
            BackgroundQueryDbModel backgroundQueryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService,
            ISummaryService summaryService) : base(
                completionPresetType,
                tag,
                backgroundQueryDbModel,
                contextBuilderFactory,
                promptContextElementBuilderFactory,
                storageService,
                httpLLMApiProviderService,
                summaryService)
        { }

        public override async Task<bool> ProcessCompletedQueryAsync()
        {
            if (!await base.ProcessCompletedQueryAsync())
            {
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                backgroundQueryDbModel.RetryCount++;
                return false;
            }

            // Deserialize the generic content into a valid CharacterSheet
            try
            {
                LLMApiResponseMessage LLMmessage = messages.LastOrDefault();
                IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextPromptInjectionForCharacterAvatarBuilder);
                if (shareableContextLink == null)
                {
                    LoggingManager.LogToFile("0075e57d-199d-4d1d-ba14-3572e715875c", $"No ShareableContextLink of type [{nameof(PromptContextPromptInjectionForCharacterAvatarBuilder)}] found.");
                    return false;
                }

                var links = JsonCommonSerializer.DeserializeFromString<ShareableNewCharacterLinks>(backgroundQueryDbModel.LinkedId);
                if (links?.CharacterId == null || links.InteractiveUserInputQueryId == null)
                {
                    // There's no fix for this
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                string promptInjectionResult = LLMResponseParser.ParseOnlyJson(messages.First().Content);
                var promptInjectionResults = JsonCommonSerializer.DeserializeFromString<IllustratorGenerationContents>(promptInjectionResult);

                if (promptInjectionResults?.Contents == null || promptInjectionResults.Contents.Count <= 0)
                {
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                // Save the promptInjection to the character
                var character = await storageService.GetCharacterByIdAsync(links.CharacterId);
                if (character != null)
                {
                    foreach (var result in promptInjectionResults.Contents)
                    {
                        var elements = character.ImageGenerationConfiguration.IllustrationMapOutfits.Where(w => w.Outfit == result.Outfit);

                        if (elements.Any())
                        {
                            foreach (var element in elements)
                            {
                                element.IllustratorPromptInjection = result.Content;
                            }
                        } else
                        {
                            character.ImageGenerationConfiguration.IllustrationMapOutfits.Add(new IllustrationMapOutfit
                            {
                                Outfit = result.Outfit,
                                IllustratorPromptInjection = result.Content
                            });
                        }
                    }

                    await storageService.UpdateCharacterAsync(character);
                }

                // Update the status of the linkedInteractiveUserInputQuery to Completed
                var linkedInteractiveUserInputQuery = await storageService.GetInteractiveUserInputQueriesAsync(g => g.InteractiveUserInputQueryId == links.InteractiveUserInputQueryId);
                if (linkedInteractiveUserInputQuery.Length == 1)
                {
                    linkedInteractiveUserInputQuery[0].Status = InteractiveUserInputStatus.Completed;
                    await storageService.UpdateInteractiveUserInputQueryAsync(linkedInteractiveUserInputQuery[0]);
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("20178b25-42ea-462d-9f28-392f53aa8b8f", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }
    }
}