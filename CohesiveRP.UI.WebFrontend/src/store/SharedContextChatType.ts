// import type { ChatMessage } from "../ResponsesDto/chat/BusinessObjects/ChatMessage";
import type { SharedContextType } from "./SharedContextType";

interface SharedContextChatType extends SharedContextType {
  chatId: string;
  defaultChatAvatar: string;
  currentUserInputValue: string;
  nbColdMessages: number | null,
  mainQueryId: string | null;// The id of the background query running to generate the ai reply
};

export type {
  SharedContextChatType
};