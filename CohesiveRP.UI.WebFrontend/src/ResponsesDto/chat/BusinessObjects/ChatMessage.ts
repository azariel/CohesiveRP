interface ChatMessage {
    messageId: string;
    messageIndex: number | null;
    content: string | null;
    thinkingContent: string | null;
    sourceType: number;
    createdAtUtc: string | null;
    summarized: boolean | null;
    characterId: string | null;
    characterName: string | null;
    personaId: string | null;
    personaName: string | null;
    avatarsFilePath: string[] | null; // zero to ten avatar path overrides for this message
    // Injected fields
    startGenerationDateTimeUtc?: string;
    startFocusedGenerationDateTimeUtc?: string;
    endFocusedGenerationDateTimeUtc?: string;
}
export type {
    ChatMessage
};
