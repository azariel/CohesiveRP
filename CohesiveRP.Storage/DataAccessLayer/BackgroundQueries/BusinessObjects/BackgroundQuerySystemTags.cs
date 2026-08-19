namespace CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects
{
    public enum BackgroundQuerySystemTags
    {
        main = 0,
        sceneTracker = 1,
        custom = 2,
        shortSummary = 3,
        mediumSummary = 4,
        longSummary = 5,
        extraSummary = 6,
        overflowSummary = 7,
        skillChecksInitiator = 8,
        sceneAnalyze = 9,// Hmm this one is unlinked and should probably be remove since we created the cohesionEnforcement ones to replace this
        dynamicCharacterCreation = 10,
        dynamicCharacterSheetCreation = 11,
        illustrationPromptInjectionForCharacterAvatar = 12,
        //cohesionEnforcement = 13,
        narrativeArchitecture = 14,
        narrativeDirection = 15,
        proseGuardian = 16,
        characterStatusUpdate = 17,
        skillChecksDescriptor = 18,
        cohesionEnforcementAnalyzer = 19,
        cohesionEnforcementWriter = 20,
    }
}
