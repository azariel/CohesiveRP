interface ChatMessage {
    messageId: string;
    content: string;
    sourceType: number;
    createdAtUtc: string;
}

export type {
    ChatMessage
};