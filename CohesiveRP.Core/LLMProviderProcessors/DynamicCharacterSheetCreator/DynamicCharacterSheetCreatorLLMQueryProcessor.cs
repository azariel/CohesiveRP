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
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.DynamicCharacterCreator
{
    internal class DynamicCharacterSheetCreatorLLMQueryProcessor : LLMQueryProcessor
    {
        public DynamicCharacterSheetCreatorLLMQueryProcessor(
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
                IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextCharacterSheetCreationBuilder);
                if (shareableContextLink == null)
                {
                    LoggingManager.LogToFile("dc9830ad-b6df-4c15-b735-e0f9b74dfe72", $"No ShareableContextLink of type [{nameof(PromptContextCharacterSheetCreationBuilder)}] found.");
                    return false;
                }

                string characterSheetJson = LLMResponseParser.ParseOnlyJson(messages.First().Content);
                CharacterSheet characterSheetFromLLMApiResponse = JsonCommonSerializer.DeserializeFromString<CharacterSheet>(characterSheetJson);

                if (string.IsNullOrWhiteSpace(characterSheetFromLLMApiResponse?.FirstName) ||
                    characterSheetFromLLMApiResponse.Kinks == null || characterSheetFromLLMApiResponse.Kinks.Length <= 1 ||
                    characterSheetFromLLMApiResponse.SecretKinks == null || characterSheetFromLLMApiResponse.SecretKinks.Length <= 1
                )
                {
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                // Add a new CharacterSheet in storage for this character
                var links = JsonCommonSerializer.DeserializeFromString<ShareableNewCharacterLinks>(backgroundQueryDbModel.LinkedId);

                if(links?.CharacterId == null || links.InteractiveUserInputQueryId == null)
                {
                    // There's no fix for this
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                CharacterSheetDbModel dbModelCharacterSheet = new()
                {
                    CharacterId = links.CharacterId,
                    CharacterSheet = characterSheetFromLLMApiResponse,
                };

                var characterSheetDbModel = await storageService.AddCharacterSheetAsync(dbModelCharacterSheet);
                if (characterSheetDbModel == null)
                {
                    LoggingManager.LogToFile("0096f909-23ce-4496-af6f-6d4fffba9d1a", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't create new characterSheet in storage.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                // Add a characterSheetInstance to the ChatId as well
                var currentCharacterSheetsInstancesInChat = await storageService.GetCharacterSheetsInstanceByChatIdAsync(backgroundQueryDbModel.ChatId);
                if (currentCharacterSheetsInstancesInChat == null)
                {
                    // There's no fix for this
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                currentCharacterSheetsInstancesInChat.CharacterSheetInstances ??= new();
                currentCharacterSheetsInstancesInChat.CharacterSheetInstances.Add(new CharacterSheetInstance
                {
                    CharacterId = links.CharacterId,
                    CharacterSheetInstanceId = Guid.NewGuid().ToString(),
                    CharacterSheet = new CharacterSheet
                    {
                        AgeGroup = characterSheetFromLLMApiResponse.AgeGroup,
                        Attractiveness = characterSheetFromLLMApiResponse.Attractiveness,
                        Behavior = characterSheetFromLLMApiResponse.Behavior,
                        BirthdayDate = characterSheetFromLLMApiResponse.BirthdayDate,
                        BodyType = characterSheetFromLLMApiResponse.BodyType,
                        BreastsSize = characterSheetFromLLMApiResponse.BreastsSize,
                        ClothesPreference = characterSheetFromLLMApiResponse.ClothesPreference,
                        CombatAffinityAttack = characterSheetFromLLMApiResponse.CombatAffinityAttack,
                        CombatAffinityDefense = characterSheetFromLLMApiResponse.CombatAffinityDefense,
                        Dislikes = characterSheetFromLLMApiResponse.Dislikes,
                        EarShape = characterSheetFromLLMApiResponse.EarShape,
                        EyeColor = characterSheetFromLLMApiResponse.EyeColor,
                        Fears = characterSheetFromLLMApiResponse.Fears,
                        FirstName = characterSheetFromLLMApiResponse.FirstName,
                        Gender = characterSheetFromLLMApiResponse.Gender,
                        Genitals = characterSheetFromLLMApiResponse.Genitals,
                        PenisSize = characterSheetFromLLMApiResponse.PenisSize,
                        GoalsForNextYear = characterSheetFromLLMApiResponse.GoalsForNextYear,
                        HairColor = characterSheetFromLLMApiResponse.HairColor,
                        HairStyle = characterSheetFromLLMApiResponse.HairStyle,
                        Height = characterSheetFromLLMApiResponse.Height,
                        Kinks = characterSheetFromLLMApiResponse.Kinks,
                        LastName = characterSheetFromLLMApiResponse.LastName,
                        Likes = characterSheetFromLLMApiResponse.Likes,
                        LongTermGoals = characterSheetFromLLMApiResponse.LongTermGoals,
                        Mannerisms = characterSheetFromLLMApiResponse.Mannerisms,
                        PathfinderAttributesValues = characterSheetFromLLMApiResponse.PathfinderAttributesValues,
                        PathfinderSkillsValues = characterSheetFromLLMApiResponse.PathfinderSkillsValues,
                        PersonalityTraits = characterSheetFromLLMApiResponse.PersonalityTraits,
                        PreferredCombatStyle = characterSheetFromLLMApiResponse.PreferredCombatStyle,
                        Profession = characterSheetFromLLMApiResponse.Profession,
                        Race = characterSheetFromLLMApiResponse.Race,
                        Relationships = characterSheetFromLLMApiResponse.Relationships,
                        Reputation = characterSheetFromLLMApiResponse.Reputation,
                        SecretKinks = characterSheetFromLLMApiResponse.SecretKinks,
                        Secrets = characterSheetFromLLMApiResponse.Secrets,
                        Sexuality = characterSheetFromLLMApiResponse.Sexuality,
                        Skills = characterSheetFromLLMApiResponse.Skills,
                        SkinColor = characterSheetFromLLMApiResponse.SkinColor,
                        SocialAnxiety = characterSheetFromLLMApiResponse.SocialAnxiety,
                        SpeechImpairment = characterSheetFromLLMApiResponse.SpeechImpairment,
                        SpeechPattern = characterSheetFromLLMApiResponse.SpeechPattern,
                        Weaknesses = characterSheetFromLLMApiResponse.Weaknesses,
                        WeaponsProficiency = characterSheetFromLLMApiResponse.WeaponsProficiency,
                    }
                });

                var characterSheetInstanceDbModel = await storageService.UpdateCharacterSheetsInstanceAsync(currentCharacterSheetsInstancesInChat);
                if (characterSheetDbModel == null)
                {
                    LoggingManager.LogToFile("43c70781-064f-493c-8dbf-4e35e992b341", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't update characterSheetInstances tied to chatId [{backgroundQueryDbModel.ChatId}] in storage.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
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
                LoggingManager.LogToFile("e1a930bf-c017-4ec8-a088-ce2abe84c371", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }
    }
}
