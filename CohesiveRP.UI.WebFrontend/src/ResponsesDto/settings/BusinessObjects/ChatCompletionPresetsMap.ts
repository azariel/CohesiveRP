import type { TagType } from "./SettingsEnums";

export interface ChatCompletionPresetMapEntry {
  type: TagType;
  chatCompletionPresetId: string;
  isDefault: boolean;
}

export interface ChatCompletionPresetsMap {
  map: ChatCompletionPresetMapEntry[];
}