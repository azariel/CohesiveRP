using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Core.Services;

namespace CohesiveRP.Core.PromptContext.Builders
{
    public class PromptContextElementBuilderFactory : IPromptContextElementBuilderFactory
    {
        private IStorageService storageService;

        public PromptContextElementBuilderFactory(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IPromptContextElementBuilder> GenerateBuilderAsync(PromptContextFormatElement contextElement)
        {
            if(contextElement?.Tag == null)
            {
                return null;
            }

            switch (contextElement.Tag)
            {
                case PromptContextFormatTag.Directive:
                    return new PromptContextDirectiveBuilder(storageService);
                case PromptContextFormatTag.World:
                    return new PromptContextWorldBuilder(storageService);
                case PromptContextFormatTag.LoreByKeywords:
                    return new PromptContextLoreByKeywordsBuilder(storageService);
                case PromptContextFormatTag.SummaryExtraTerm:
                    return new PromptContextSummaryExtraTermBuilder(storageService);
                case PromptContextFormatTag.SummaryLongTerm:
                    return new PromptContextSummaryLongTermBuilder(storageService);
                case PromptContextFormatTag.SummaryMediumTerm:
                    return new PromptContextSummaryMediumTermBuilder(storageService);
                case PromptContextFormatTag.SummaryShortTerm:
                    return new PromptContextSummaryShortTermBuilder(storageService);
                case PromptContextFormatTag.LoreByQuery:
                    return new PromptContextLoreByQueryBuilder(storageService);
                case PromptContextFormatTag.RelevantCharacters:
                    return new PromptContextRelevantCharactersBuilder(storageService);
                case PromptContextFormatTag.LastXMessages:
                    return new PromptContextLastXMessagesBuilder(storageService);
                case PromptContextFormatTag.SceneTracker:
                    return new PromptContextSceneTrackerBuilder(storageService);
                case PromptContextFormatTag.CurrentObjective:
                    return new PromptContextCurrentObjectiveBuilder(storageService);
                case PromptContextFormatTag.LastUserMessage:
                    return new PromptContextLastUserMessageBuilder(storageService);
                case PromptContextFormatTag.BehavioralInstructions:
                    return new PromptContextBehavioralInstructionsBuilder(storageService);
                default:
                    throw new Exception($"Unhandled [{contextElement.Tag}].");
            }
        }
    }
}
