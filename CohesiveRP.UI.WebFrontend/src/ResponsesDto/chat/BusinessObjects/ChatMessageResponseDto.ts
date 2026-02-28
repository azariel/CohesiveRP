import type { ServerApiResponseDto } from "../../ServerApiResponseDto";
import type { ChatMessage } from "./ChatMessage";

interface ChatMessageResponseDto extends ServerApiResponseDto {
    messageObj: ChatMessage;
}

export type {
    ChatMessageResponseDto
};