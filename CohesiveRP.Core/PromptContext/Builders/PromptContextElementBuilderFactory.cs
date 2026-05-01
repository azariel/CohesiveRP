using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.PromptContext.Builders.Illustrator.MainCharacterAvatar;
using CohesiveRP.Core.PromptContext.Builders.Pathfinder;
using CohesiveRP.Core.PromptContext.Builders.Pathfinder.RelevantCharacters;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Settings;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public class PromptContextElementBuilderFactory : IPromptContextElementBuilderFactory
    {
        private IStorageService storageService;

        public PromptContextElementBuilderFactory(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IPromptContextElementBuilder> GenerateBuilderAsync(PromptContextFormatElement contextElement, GlobalSettingsDbModel settings, ChatDbModel chatDbModel, BackgroundQueryDbModel backgroundQuery, BackgroundQuerySystemTags tag, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            if (contextElement?.Tag == null)
            {
                return null;
            }

            switch (contextElement.Tag)
            {
                case PromptContextFormatTag.Directive:
                    return new PromptContextDirectiveBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.World:
                    return new PromptContextWorldBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.LoreByKeywords:
                    return new PromptContextLoreByKeywordsBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.SummaryExtraTerm:
                    return new PromptContextSummaryExtraTermBuilder(storageService, contextElement, settings, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.SummaryLongTerm:
                    return new PromptContextSummaryLongTermBuilder(storageService, contextElement, settings, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.SummaryMediumTerm:
                    return new PromptContextSummaryMediumTermBuilder(storageService, contextElement, settings, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.SummaryShortTerm:
                    return new PromptContextSummaryShortTermBuilder(storageService, contextElement, settings, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.LoreByQuery:
                    return new PromptContextLoreByQueryBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.RelevantCharacters:
                    return new PromptContextRelevantCharactersBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.DirectCharactersDescription:
                    return new PromptContextDirectCharactersInjectionBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.LastXMessages:
                    return new PromptContextLastXMessagesBuilder(storageService, contextElement, settings, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.SceneTracker:
                    return new PromptContextSceneTrackerBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.SceneAnalyze:
                    return new PromptContextSceneAnalyzerInstrBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.SceneTrackerInstructions:
                    return new PromptContextSceneTrackerInstrBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.CurrentObjective:
                    return new PromptContextCurrentObjectiveBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.LastUserMessage:
                    return new PromptContextLastUserMessageBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.BehavioralInstructions:
                    return new PromptContextBehavioralInstructionsBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.LastXMessagesToSummarize:
                    return new PromptContextLastXMessagesToSummarizeBuilder(storageService, contextElement, settings, chatDbModel, backgroundQuery?.LinkedId, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.LastUnsummarizedMessages:
                    return new PromptContextLastUnsummarizedMessagesBuilder(storageService, contextElement, settings, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.OverflowingSummariesToSummarize:
                    return new PromptContextSummarizeSummariesBuilder(storageService, contextElement, settings, chatDbModel, tag, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.PathfinderSkillChecksInitiator:// Initiate a call to the LLM to get what skillchecks must be done
                    return new SkillChecksInitiatorBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.PathfinderSkillChecksResults:// Add the skillChecks result to the end prompt
                    return new SkillChecksResultsBuilder(storageService, contextElement, chatDbModel, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.CharacterCreation:
                    return new PromptContextCharacterCreationBuilder(storageService, contextElement, chatDbModel, backgroundQuery?.LinkedId, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.CharacterSheetCreation:
                    return new PromptContextCharacterSheetCreationBuilder(storageService, contextElement, chatDbModel, backgroundQuery?.LinkedId, personaLinkedToChat, charactersLinkedToChat);
                case PromptContextFormatTag.IllustrationPromptInjectionForCharacterAvatar:
                    return new PromptContextPromptInjectionForCharacterAvatarBuilder(storageService, contextElement, chatDbModel, backgroundQuery?.LinkedId, personaLinkedToChat, charactersLinkedToChat);
                default:
                    throw new Exception($"Unhandled [{contextElement.Tag}].");
            }
        }
    }
}
