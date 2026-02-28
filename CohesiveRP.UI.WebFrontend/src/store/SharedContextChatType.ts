import type { ChatMessage } from "../ResponsesDto/chat/BusinessObjects/ChatMessage";
import type { SharedContextType } from "./SharedContextType";

interface SharedContextChatType extends SharedContextType {
  chatId: string;
  messages: ChatMessage[];
};

export type {
  SharedContextChatType
};