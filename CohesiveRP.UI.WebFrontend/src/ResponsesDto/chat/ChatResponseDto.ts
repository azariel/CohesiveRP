import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface ChatResponseDto extends ServerApiResponseDto  {
    chatId: string | null;
    characterIds: string[] | null;
    lorebookIds: string[] | null;
    name: string | null;
    lastActivityDateTime: Date | null;
}

export type {
    ChatResponseDto
};