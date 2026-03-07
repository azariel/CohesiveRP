using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public class PromptContextElementBuilderFactory : IPromptContextElementBuilderFactory
    {
        private IStorageService storageService;

        public PromptContextElementBuilderFactory(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IPromptContextElementBuilder> GenerateBuilderAsync(PromptContextFormatElement contextElement, PromptContextSettings settings, ChatDbModel chatDbModel)
        {
            if (contextElement?.Tag == null)
            {
                return null;
            }

            switch (contextElement.Tag)
            {
                case PromptContextFormatTag.Directive:
                    return new PromptContextDirectiveBuilder(storageService, contextElement, chatDbModel);
                case PromptContextFormatTag.World:
                    return new PromptContextWorldBuilder(storageService, contextElement, chatDbModel);
                case PromptContextFormatTag.LoreByKeywords:
                    return new PromptContextLoreByKeywordsBuilder(storageService, contextElement, chatDbModel);
                case PromptContextFormatTag.SummaryExtraTerm:
                    return new PromptContextSummaryExtraTermBuilder(storageService, contextElement, settings, chatDbModel);
                case PromptContextFormatTag.SummaryLongTerm:
                    return new PromptContextSummaryLongTermBuilder(storageService, contextElement, settings, chatDbModel);
                case PromptContextFormatTag.SummaryMediumTerm:
                    return new PromptContextSummaryMediumTermBuilder(storageService, contextElement, settings, chatDbModel);
                case PromptContextFormatTag.SummaryShortTerm:
                    return new PromptContextSummaryShortTermBuilder(storageService, contextElement, settings, chatDbModel);
                case PromptContextFormatTag.LoreByQuery:
                    return new PromptContextLoreByQueryBuilder(storageService, contextElement, chatDbModel);
                case PromptContextFormatTag.RelevantCharacters:
                    return new PromptContextRelevantCharactersBuilder(storageService, contextElement, chatDbModel);
                case PromptContextFormatTag.LastXMessages:
                    return new PromptContextLastXMessagesBuilder(storageService, contextElement, settings, chatDbModel);
                case PromptContextFormatTag.SceneTracker:
                    return new PromptContextSceneTrackerBuilder(storageService, contextElement, chatDbModel);
                case PromptContextFormatTag.CurrentObjective:
                    return new PromptContextCurrentObjectiveBuilder(storageService, contextElement, chatDbModel);
                case PromptContextFormatTag.LastUserMessage:
                    return new PromptContextLastUserMessageBuilder(storageService, contextElement, chatDbModel);
                case PromptContextFormatTag.BehavioralInstructions:
                    return new PromptContextBehavioralInstructionsBuilder(storageService, contextElement, chatDbModel);
                case PromptContextFormatTag.LastXMessagesToSummarize:
                    return new PromptContextLastXMessagesToSummarizeBuilder(storageService, contextElement, settings, chatDbModel);
                default:
                    throw new Exception($"Unhandled [{contextElement.Tag}].");
            }
        }
    }
}
