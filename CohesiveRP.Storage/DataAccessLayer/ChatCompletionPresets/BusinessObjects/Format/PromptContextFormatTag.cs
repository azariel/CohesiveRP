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
        CurrentObjective = 11,
        LastUserMessage = 12,
        BehavioralInstructions = 13,
        LastXMessagesToSummarize = 14,
    }
}
