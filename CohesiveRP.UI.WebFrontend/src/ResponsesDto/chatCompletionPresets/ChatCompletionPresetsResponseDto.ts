import type { ChatCompletionPreset } from "./ChatCompletionPreset";

export interface ChatCompletionPresetsResponseDto {
  code: number;
  chatCompletionPresets: ChatCompletionPreset[];
}