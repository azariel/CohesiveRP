interface ChatMessage {
    messageId: string;
    content: string | null;
    sourceType: number;
    createdAtUtc: string | null;
}

export type {
    ChatMessage
};