interface ChatMessage {
    messageId: string;
    messageIndex: number | null;
    content: string | null;
    sourceType: number;
    createdAtUtc: string | null;
}

export type {
    ChatMessage
};