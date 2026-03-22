// ─── Enum ─────────────────────────────────────────────────────────────────────

export type PromptContextFormatTag =
  | "Directive"
  | "World"
  | "LoreByKeywords"
  | "SummaryExtraTerm"
  | "SummaryLongTerm"
  | "SummaryMediumTerm"
  | "SummaryShortTerm"
  | "LoreByQuery"
  | "RelevantCharacters"
  | "LastXMessages"
  | "SceneTracker"
  | "SceneTrackerInstructions"
  | "CurrentObjective"
  | "LastUserMessage"
  | "BehavioralInstructions"
  | "LastXMessagesToSummarize"
  | "LastUnsummarizedMessages"
  | "OverflowingSummariesToSummarize"
  | "DirectCharactersDescription";

export const ALL_FORMAT_TAGS: PromptContextFormatTag[] = [
  "Directive",
  "World",
  "LoreByKeywords",
  "SummaryExtraTerm",
  "SummaryLongTerm",
  "SummaryMediumTerm",
  "SummaryShortTerm",
  "LoreByQuery",
  "RelevantCharacters",
  "LastXMessages",
  "SceneTracker",
  "SceneTrackerInstructions",
  "CurrentObjective",
  "LastUserMessage",
  "BehavioralInstructions",
  "LastXMessagesToSummarize",
  "LastUnsummarizedMessages",
  "OverflowingSummariesToSummarize",
  "DirectCharactersDescription",
];

// Readable display labels (camelCase → spaced)
export const FORMAT_TAG_LABELS: Record<PromptContextFormatTag, string> = {
  Directive:                        "Directive",
  World:                            "World",
  LoreByKeywords:                   "Lore by Keywords",
  SummaryExtraTerm:                 "Summary — Extra Term",
  SummaryLongTerm:                  "Summary — Long Term",
  SummaryMediumTerm:                "Summary — Medium Term",
  SummaryShortTerm:                 "Summary — Short Term",
  LoreByQuery:                      "Lore by Query",
  RelevantCharacters:               "Relevant Characters",
  LastXMessages:                    "Last X Messages",
  SceneTracker:                     "Scene Tracker",
  SceneTrackerInstructions:         "Scene Tracker Instructions",
  CurrentObjective:                 "Current Objective",
  LastUserMessage:                  "Last User Message",
  BehavioralInstructions:           "Behavioral Instructions",
  LastXMessagesToSummarize:         "Last X Messages to Summarize",
  LastUnsummarizedMessages:         "Last Unsummarized Messages",
  OverflowingSummariesToSummarize:  "Overflowing Summaries to Summarize",
  DirectCharactersDescription:      "Direct Characters Description",
};

// ─── Domain objects ───────────────────────────────────────────────────────────

export interface PromptContextFormatElementOptions {
  format: string;
}

export interface PromptContextFormatElement {
  tag: PromptContextFormatTag;
  options: PromptContextFormatElementOptions;
}

export interface GlobalPromptContextFormat {
  orderedElementsWithinTheGlobalPromptContext: PromptContextFormatElement[];
}

// ─── API DTOs ─────────────────────────────────────────────────────────────────

/** Shape returned by GET api/chatCompletionPresets (list) */
export interface ChatCompletionPresetSummary {
  chatCompletionPresetId: string;
  name: string;
}

export interface ChatCompletionPresetsListResponseDto {
  code: number;
  chatCompletionPresets: ChatCompletionPresetSummary[];
}

export interface ChatCompletionPresetDetails {
  chatCompletionPresetId: string;
  name: string;
  format: GlobalPromptContextFormat;
}

/** Shape returned by GET api/chatCompletionPresets/{id} */
export interface ChatCompletionPresetDetailResponseDto {
  code: number;
  chatCompletionPreset: ChatCompletionPresetDetails;
}

/** Body sent to POST / PUT */
export interface UpsertChatCompletionPresetRequestDto {
  chatCompletionPresetId: string;
  name: string;
  format: GlobalPromptContextFormat;
}
