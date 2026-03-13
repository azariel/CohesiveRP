import styles from "./ChatComponent.module.css";
import { useEffect, useRef } from "react";
import ChatMessageComponent from "./message/ChatMessageComponent";
import UserInputComponent from "./userInput/UserInputComponent";
import { deleteFromServerApiAsync, getFromServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ChatMessagesResponseDto } from "../../../../ResponsesDto/chat/ChatMessagesResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../store/SharedContextChatType";
import { useChatMessages } from "../../../../store/MessagesStoreContext";

export default function ChatComponent() {
  const messagesRef = useRef<HTMLDivElement>(null);
  const { activeModule } = sharedContext<SharedContextChatType>();
  const [messages, setMessages] = useChatMessages(activeModule?.chatId);
  const didComponentMountAlready = useRef(false);

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;

    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        if(!activeModule?.chatId) {
          console.error(`Couldn't load chat. ChatId was undefined.`);
          return;
        }

        const response: ChatMessagesResponseDto | null = await getFromServerApiAsync<ChatMessagesResponseDto>(`api/chat/${activeModule.chatId}/messages/hot`);
        
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch specific chat messages failed. [${JSON.stringify(serverApiException)}]`);
          return;
        }

        setMessages(() => response.messages ?? []);
        console.log(`Specific chat messages fetched successfully.`);
        setTimeout(() => {
          if (messagesRef?.current)
            messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
        }, 200);
      } catch (error) {
        console.error("Fetch messages error:", error);
      }
    };

    fetchData();
    
  }, []);

  useEffect(() => {
    if (messagesRef?.current)
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
  }, []);

  const handleSaveMessage = async (messageId: string, newContent: string) => {
    const updatedMessage = messages.find((m) => m.messageId === messageId);
    if (!updatedMessage)
      return;

  
    const payload = { ...updatedMessage, content: newContent };

    // Optimistic update
    setMessages((prev) => prev.map((m) => m.messageId === messageId ? payload : m));

    // Save to backend
    const response = await putToServerApiAsync(`api/chat/${activeModule.chatId}/messages/${messageId}`, updatedMessage);
    const serverApiException = response as ServerApiExceptionResponseDto | null;
    if (!response || serverApiException?.message) {
      console.error(`Updating message failed. [${JSON.stringify(serverApiException)}]`);
    }
  };

  const handleDeleteMessage = async (messageId: string) => {
    // Optimistic removal from local store
    setMessages((prev) => prev.filter((m) => m.messageId !== messageId));

    // Delete on backend
    const response = await deleteFromServerApiAsync(`api/chat/${activeModule.chatId}/messages/${messageId}`);
    const serverApiException = response as ServerApiExceptionResponseDto | null;
    if (!response || serverApiException?.message) {
      console.error(`Deleting message failed. [${JSON.stringify(serverApiException)}]`);

      // roll back optimistic removal on failure
    }
  };

  // const lastMessageIndex = (activeModule?.messages?.length ?? 0) - 1;
  // const editableMessageIndex = lastMessageIndex - 3;
  const isSendingMessage = !!activeModule?.mainQueryId; // blocked while AI is replying

  return (
    <main className={styles.chatComponent}>
      <div className={styles.messagesContainer} ref={messagesRef}>
        {messages.length > 0 ? (
          messages.map((message, index) => (
            <ChatMessageComponent
              key={message.messageId}  // ← use stable id, not index
              messagesRef={messagesRef}
              message={message}
              defaultChatAvatarId={activeModule.defaultChatAvatar ?? ""}
              enableDeleteBtn={index >= messages.length - 1}
              enableSwipeBtn={index >= messages.length - 1}
              isEditable={!isSendingMessage && !message.summarized}
              onSave={handleSaveMessage}
              onDelete={handleDeleteMessage}
            />
          ))
        ) : (
          <p />
        )}
        <div className={styles.userInputContainer}>
          <UserInputComponent messagesRef={messagesRef} defaultChatAvatarId={activeModule.defaultChatAvatar ?? ""} />
        </div>
      </div>
    </main>
  );
}