import type { ChatMessage } from "../ResponsesDto/chat/BusinessObjects/ChatMessage";
import type { SharedContextType } from "./SharedContextType";

interface SharedContextChatType extends SharedContextType {
  chatId: string;
  messages: ChatMessage[];
  // setMessages: (messages: ChatMessageResponseDto[]) => void;
  // addMessage: (message: ChatMessageResponseDto) => void;
};

export type {
  SharedContextChatType
};