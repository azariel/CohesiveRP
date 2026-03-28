interface ChatMessage {
    messageId: string;
    messageIndex: number | null;
    content: string | null;
    sourceType: number;
    createdAtUtc: string | null;
    summarized: boolean | null;
    characterId: string | null;
    characterName: string | null;
    personaId: string | null;
    personaName: string | null;
    avatarFilePath: string | null;// if a specific message has an avatar override
}

export type {
    ChatMessage
};