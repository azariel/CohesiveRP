import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface SettingsResponseDto extends ServerApiResponseDto  {
    LLMProviders: string | null
}

export type {
    SettingsResponseDto
};