import type { ServerApiResponseDto } from "../../ServerApiResponseDto";

export interface InteractiveUserInputQueriesResponseDto extends ServerApiResponseDto {
  queries: InteractiveUserInputQueryResponse[];
}

export interface InteractiveUserInputQueryResponse {
  queryId: string;
  chatId: string;
  sceneTrackerId: string;
  createdAtUtc: string | null;
  metadata: string;
  type: InteractiveUserInputType;
  status: InteractiveUserInputStatus;
}

export const InteractiveUserInputType = {
  NewCharacterDetected: "NewCharacterDetected",
} as const;
export type InteractiveUserInputType = typeof InteractiveUserInputType[keyof typeof InteractiveUserInputType];

export const InteractiveUserInputStatus = {
  WaitingUserInput: "WaitingUserInput",
  Processing: "Processing",
  Completed: "Completed",
  Error: "Error",
} as const;
export type InteractiveUserInputStatus = typeof InteractiveUserInputStatus[keyof typeof InteractiveUserInputStatus];

export interface PutInteractiveUserInputQueryRequest {
  queryId: string;
  chatId: string;
  sceneTrackerId: string;
  metadata: string;
  type: InteractiveUserInputType;
  status: InteractiveUserInputStatus;
  userChoice: boolean;
}
