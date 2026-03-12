import type { ChatMessage } from "../ResponsesDto/chat/BusinessObjects/ChatMessage";
import type { SharedContextType } from "./SharedContextType";

interface SharedContextChatType extends SharedContextType {
  chatId: string;
  defaultChatAvatar: string;
  messages: ChatMessage[];
  mainQueryId: string | null;// The id of the background query running to generate the ai reply
};

export type {
  SharedContextChatType
};