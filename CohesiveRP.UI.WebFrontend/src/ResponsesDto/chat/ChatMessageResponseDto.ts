import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { ChatMessage } from "./BusinessObjects/ChatMessage";

interface ChatMessageResponseDto extends ServerApiResponseDto {
    messageObj: ChatMessage;
    mainQueryId: string;
}

export type {
    ChatMessageResponseDto
};