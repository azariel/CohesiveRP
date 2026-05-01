using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Illustrator.MainCharacterAvatar;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.Illustrator.MainCharacterAvatar
{
    internal class IllustrationPromptInjectionForCharacterAvatarLLMQueryProcessor: LLMQueryProcessor
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

                string promptInjectionResult = LLMResponseParser.ParseOnlyJson(messages.First().Content);
                StringResultWrapper promptInjectionResultWrapper = JsonCommonSerializer.DeserializeFromString<StringResultWrapper>(promptInjectionResult);

                if (string.IsNullOrWhiteSpace(promptInjectionResultWrapper?.Content))
                {
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                // Save the promptInjection to the character
                //var character = await storageService.GetCharacterByIdAsync(backgroundQueryDbModel.LinkedId);
                //if(character != null)
                //{
                //    character.ImageGenerationConfiguration. = promptInjectionResultWrapper.Content;
                //    await storageService.UpdateCharacterAsync(character);
                //}

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