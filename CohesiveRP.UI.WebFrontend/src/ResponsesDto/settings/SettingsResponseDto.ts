import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface SettingsResponseDto extends ServerApiResponseDto  {
    llmProviders: string | null
}

export type {
    SettingsResponseDto
};