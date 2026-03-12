interface ChatMessage {
    messageId: string;
    messageIndex: number | null;
    content: string | null;
    sourceType: number;
    createdAtUtc: string | null;
    summarized: boolean | null;
    characterId: string | null;
    avatarId: string | null;// if a specific message has an avatar override
}

export type {
    ChatMessage
};