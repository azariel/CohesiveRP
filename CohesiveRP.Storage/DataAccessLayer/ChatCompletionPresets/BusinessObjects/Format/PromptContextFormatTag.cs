namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format
{
    public enum PromptContextFormatTag
    {
        Directive = 0,
        World = 1,
        LoreByKeywords = 2,
        SummaryExtraTerm = 3,
        SummaryLongTerm = 4,
        SummaryMediumTerm = 5,
        SummaryShortTerm = 6,
        LoreByQuery = 7,
        RelevantCharacters = 8,
        LastXMessages = 9,
        SceneTracker = 10,
        SceneTrackerInstructions = 11,
        CurrentObjective = 12,
        LastUserMessage = 13,
        BehavioralInstructions = 14,
        LastXMessagesToSummarize = 15,
        LastUnsummarizedMessages = 16,
        OverflowingSummariesToSummarize = 17,
        DirectCharactersDescription = 18,
        SkillChecksInitiator = 19,
    }
}
