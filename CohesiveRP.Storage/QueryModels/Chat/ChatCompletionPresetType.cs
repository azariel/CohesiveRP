namespace CohesiveRP.Storage.QueryModels.Chat
{
    public enum ChatCompletionPresetType
    {
        Main = 0,
        Summarize = 1,
        SummariesMerge = 2,
        SceneTracker = 3,
        SkillChecksInitiator = 4,
        SPECIAL_CharacterSheetGeneration = 5,
        SceneAnalyze = 6,
        DynamicCharacterCreation = 7,
        DynamicCharacterSheetCreation = 8,
        IllustrationPromptInjectionForCharacterAvatar = 9,
        // ChatAdditions
        CohesionEnforcement = 10,
        NarrativeArchitecture = 11,
        NarrativeDirection = 12,
        ProseGuardian = 13,
    }
}
