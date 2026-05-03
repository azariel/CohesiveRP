using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.LLMProviderProcessors.DynamicCharacterCreator.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Core.Utils.Characters;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.DynamicCharacterCreator
{
    internal class DynamicCharacterCreatorLLMQueryProcessor : LLMQueryProcessor
    {
        public DynamicCharacterCreatorLLMQueryProcessor(
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

            // Deserialize the generic content into a valid Character
            try
            {
                LLMApiResponseMessage LLMmessage = messages.LastOrDefault();
                IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextCharacterCreationBuilder);
                if (shareableContextLink == null)
                {
                    LoggingManager.LogToFile("a895742d-afff-4f93-97da-38aa4aee66a9", $"No ShareableContextLink of type [{nameof(PromptContextCharacterCreationBuilder)}] found.");
                    return false;
                }

                string characterJson = LLMResponseParser.ParseOnlyJson(messages.First().Content);
                Character characterFromLLMApi = JsonCommonSerializer.DeserializeFromString<Character>(characterJson);

                if (string.IsNullOrWhiteSpace(characterFromLLMApi?.Description) || string.IsNullOrWhiteSpace(characterFromLLMApi?.Name))
                {
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                // Add a brand new character
                string linkedInteractiveUserInputQueryId = backgroundQueryDbModel.LinkedId;
                AddCharacterQueryModel dbModel = new()
                {
                    Creator = "CoherentRp",
                    CreatorNotes = "Character was inferred from the story context and created dynamically.",
                    Tags = ["AUTO-GENERATED"],
                    Description = characterFromLLMApi.Description,
                    Name = characterFromLLMApi.Name.Trim(),
                    ImageGenerationConfiguration = new(),
                };

                CharacterDbModel characterDbModel = await storageService.AddCharacterAsync(dbModel);
                if (characterDbModel == null)
                {
                    LoggingManager.LogToFile("d47f6cad-936d-42e8-8789-f829600ee932", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't create new character in storage.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                CharacterUtils.CreateCharacterAssets(characterDbModel);

                // Update the chat to include that character
                var chat = await storageService.GetChatAsync(backgroundQueryDbModel.ChatId);
                if (chat != null)
                {
                    if (chat.CharacterIds.All(a => a != characterDbModel.CharacterId))
                    {
                        chat.CharacterIds.Add(characterDbModel.CharacterId);
                    }

                    await storageService.UpdateChatAsync(chat);
                }

                // At this point, the character has been created successfully in the storage, we need to queue a backgroundQuery to create the related CharacterSheet and then the CharacterSheetInstance to link the character to the chat.
                await QueueBackgroundQueryToCreateCharacterSheetAsync(characterDbModel);

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("e1a930bf-c017-4ec8-a088-ce2abe84c371", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }

        private async Task QueueBackgroundQueryToCreateCharacterSheetAsync(CharacterDbModel characterDbModel)
        {
            if (characterDbModel == null)
                return;

            CreateBackgroundQueryQueryModel addCharacterSheetForCharacterQueryModel = new()
            {
                ChatId = backgroundQueryDbModel.ChatId,
                Priority = BackgroundQueryPriority.Lowest,
                LinkedId = JsonCommonSerializer.SerializeToString(new ShareableNewCharacterLinks { InteractiveUserInputQueryId = backgroundQueryDbModel.LinkedId, CharacterId = characterDbModel.CharacterId }),// Keep link to the initial InteractiveUserInputQuery
                Tags = [BackgroundQuerySystemTags.dynamicCharacterSheetCreation.ToString()],
                DependenciesTags = Enum.GetValues<BackgroundQuerySystemTags>()// this one is blocked by basically ANYTHING except the same type
                    .Where(w => w != BackgroundQuerySystemTags.dynamicCharacterSheetCreation)
                    .Select(s => s.ToString())
                    .ToList(),
            };

            await storageService.AddBackgroundQueryAsync(addCharacterSheetForCharacterQueryModel);
        }
    }
}
