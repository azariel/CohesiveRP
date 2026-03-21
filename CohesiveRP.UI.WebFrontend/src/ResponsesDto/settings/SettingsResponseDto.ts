import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { ChatCompletionPresetsMap } from "./BusinessObjects/ChatCompletionPresetsMap";
import type { LLMProviderSettings } from "./BusinessObjects/LLMProviderSettings";
import type { SummarySettings } from "./BusinessObjects/SummarySettings";

interface SettingsResponseDto extends ServerApiResponseDto  {
    llmProviders: LLMProviderSettings[] | null;
    chatCompletionPresetsMap: ChatCompletionPresetsMap[] | null;
    summary: SummarySettings | null;
}

export type {
    SettingsResponseDto
};