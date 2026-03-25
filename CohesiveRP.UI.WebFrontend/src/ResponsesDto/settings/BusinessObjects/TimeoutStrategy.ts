import type { TimeoutStrategyType } from "./SettingsEnums";

export interface TimeoutStrategy {
  type: TimeoutStrategyType;
  retries: number;
}