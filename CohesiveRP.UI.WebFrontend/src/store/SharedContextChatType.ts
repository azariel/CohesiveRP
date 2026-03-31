// import type { ChatMessage } from "../ResponsesDto/chat/BusinessObjects/ChatMessage";
import type { SharedContextType } from "./SharedContextType";

interface SharedContextChatType extends SharedContextType {
  isSceneTrackerOpened: boolean | null;
  chatId: string;
  lastPlayerMessageId: string;
  currentUserInputValue: string;
  nbColdMessages: number | null,
  mainQueryId: string | null;// The id of the background query running to generate the ai reply
  sceneTrackerRefreshToken: number;// To refresh the sceneTracker once the backend has refreshed it (after the user sent a new message)
  sceneTrackerRefreshing: boolean;// sceneTracker is currently refreshing
};

export type {
  SharedContextChatType
};