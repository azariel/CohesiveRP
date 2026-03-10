interface ChatMessage {
    messageId: string;
    messageIndex: number | null;
    content: string | null;
    sourceType: number;
    createdAtUtc: string | null;
    summarized: boolean | null;
}

export type {
    ChatMessage
};