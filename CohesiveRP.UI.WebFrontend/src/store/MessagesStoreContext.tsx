import { createContext, useContext, useState } from "react";
import type { ReactNode } from "react";
import type { ChatMessage } from "../ResponsesDto/chat/BusinessObjects/ChatMessage";

type MessagesMap = Record<string, ChatMessage[]>;

type MessagesStoreType = {
  getMessages: (chatId: string) => ChatMessage[];
  setMessages: (chatId: string, updater: (prev: ChatMessage[]) => ChatMessage[]) => void;
};

const MessagesStoreContext = createContext<MessagesStoreType | null>(null);

export const MessagesStoreProvider = ({ children }: { children: ReactNode }) => {
  const [store, setStore] = useState<MessagesMap>({});

  const getMessages = (chatId: string): ChatMessage[] =>
    store[chatId] ?? [];

  const setMessages = (chatId: string, updater: (prev: ChatMessage[]) => ChatMessage[]) =>
    setStore((prev) => ({
      ...prev,
      [chatId]: updater(prev[chatId] ?? []),
    }));

  return (
    <MessagesStoreContext.Provider value={{ getMessages, setMessages }}>
      {children}
    </MessagesStoreContext.Provider>
  );
};

export function useMessagesStore() {
  const ctx = useContext(MessagesStoreContext);
  if (!ctx) throw new Error("useMessagesStore must be used within MessagesStoreProvider");
  return ctx;
}

// Convenience hook for a single chat — only re-renders when that chat's messages change
export function useChatMessages(chatId: string | undefined) {
  const { getMessages, setMessages } = useMessagesStore();
  const messages = chatId ? getMessages(chatId) : [];
  const update = (updater: (prev: ChatMessage[]) => ChatMessage[]) => {
    if (chatId) setMessages(chatId, updater);
  };
  return [messages, update] as const;
}