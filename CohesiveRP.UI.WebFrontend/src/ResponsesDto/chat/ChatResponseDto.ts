import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface ChatResponseDto extends ServerApiResponseDto  {
    id: string;
    name: string | null;
    lastActivityDateTime: Date | null;
}

export type {
    ChatResponseDto
};