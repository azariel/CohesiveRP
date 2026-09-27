interface LorebookEntry {
    keys: string[] | null;
    secondaryKeys: string[] | null;
    tags: string[] | null;
    content: string | null;
    comment: string | null;
    entryId: string | null;
    name: string | null;
    enabled: boolean;
    insertionOrder: number;
    useRegex: boolean;
    constant: boolean;
    depth: number;
    selectiveLogicBetweenKeysAndSecondaryKeys: number;
    probabilityPercentage: number;
    positionInPrompt: number;
    stickyForNbMessages: number;
    cooldown: number;
    delay: number;
    caseSensitive: boolean;
    vectorized: boolean;
    excludeRecursion: boolean;
    preventRecursion: boolean;
    onlyTriggeredByRecursion: boolean;
    matchWholeWord: boolean;
    ignoreTokensBudget: boolean;
}

export type {
    LorebookEntry
};