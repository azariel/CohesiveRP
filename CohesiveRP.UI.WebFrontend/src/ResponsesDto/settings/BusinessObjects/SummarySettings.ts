export interface ShortSummaryConfig {
  nbMessageInChunk: number;
  maxShortTermSummaryTokens: number;
}

export interface ExtensibleSummaryConfig {
  summarizeLastXTokens: number;
  maxTotalSummariesTokens: number;
}

export interface OverflowSummaryConfig {
  summarizeLastXTokens: number;
  maxOverflowSummaryTokens: number;
}

export interface SummarySettings {
  shortConfig: ShortSummaryConfig;
  mediumConfig: ExtensibleSummaryConfig;
  longConfig: ExtensibleSummaryConfig;
  extraConfig: ExtensibleSummaryConfig;
  overflowConfig: OverflowSummaryConfig;
  nbRawMessagesToKeepInContext: number;
  hotMessagesAmountLimit: number;
  coldMessagesAmountLimit: number;
}
