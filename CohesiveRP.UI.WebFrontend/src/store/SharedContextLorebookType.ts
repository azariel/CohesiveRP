// import type { ChatMessage } from "../ResponsesDto/chat/BusinessObjects/ChatMessage";
import type { SharedContextType } from "./SharedContextType";

interface SharedContextLorebookType extends SharedContextType {
  selectedLorebookId: string;
};

export type {
  SharedContextLorebookType
};