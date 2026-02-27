import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { ChatMessageResponseDto } from "./BusinessObjects/ChatMessageResponseDto"

interface ChatMessagesResponseDto extends ServerApiResponseDto  {
    messages: ChatMessageResponseDto[] | null
}

export type {
    ChatMessagesResponseDto
};