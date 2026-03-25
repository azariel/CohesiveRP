import type { LlmProviderType, PriorityType, TagType } from "./SettingsEnums";
import type { TimeoutStrategy } from "./TimeoutStrategy";

export interface LLMProviderSettings {
  providerConfigId: string;
  name: string;
  apiUrl: string;
  type: LlmProviderType;
  concurrencyLimit: number;
  model: string;
  priority: PriorityType;
  tags: TagType[];
  timeoutStrategy: TimeoutStrategy;
}