import type { ChatCompletionPreset } from "./ChatCompletionPreset";

export interface ChatCompletionPresetsSettingsResponseDto {
  code: number;
  chatCompletionPresets: ChatCompletionPreset[];
}