import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface SelectableChatResponseDto extends ServerApiResponseDto  {
    chatId: string;
    name: string | null;
    lastActivityDateTime: Date | null;
    avatarCharacterId: string;
}

export type {
    SelectableChatResponseDto
};