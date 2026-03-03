import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { ChatMessage } from "./BusinessObjects/ChatMessage"

interface ChatMessagesResponseDto extends ServerApiResponseDto  {
    messages: ChatMessage[] | null
}

export type {
    ChatMessagesResponseDto
};