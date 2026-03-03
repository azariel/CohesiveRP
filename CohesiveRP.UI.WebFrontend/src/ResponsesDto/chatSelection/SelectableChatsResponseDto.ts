import type { ServerApiResponseDto } from "../ServerApiResponseDto"
import type { SelectableChatResponseDto } from "./SelectableChatResponseDto"

interface SelectableChatsResponseDto extends ServerApiResponseDto {
    chats: SelectableChatResponseDto[]
}

export type {
    SelectableChatsResponseDto
};