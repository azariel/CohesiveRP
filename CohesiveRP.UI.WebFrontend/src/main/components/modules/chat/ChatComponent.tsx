import styles from "./ChatComponent.module.css";
import { useEffect, useRef } from "react";
import ChatMessageComponent from "./message/ChatMessageComponent";
import UserInputComponent from "./userInput/UserInputComponent";
import { getFromServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ChatMessagesResponseDto } from "../../../../ResponsesDto/chat/ChatMessagesResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../store/SharedContextChatType";

export default function ChatComponent() {
  const messagesRef = useRef<HTMLDivElement>(null);
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const didComponentMountAlready = useRef(false);

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;
    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        let chatModule = activeModule as SharedContextChatType;
        const response: ChatMessagesResponseDto | null = await getFromServerApiAsync<ChatMessagesResponseDto>(`api/chat/${chatModule.chatId}/messages/hot`);
        
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch specific chat failed. [${JSON.stringify(serverApiException)}]`);
          return;
        }

        setActiveModule({ ...activeModule, messages: response.messages ?? [] });
        console.log(`Specific chat fetched successfully.`);
        setTimeout(() => {
          if (messagesRef?.current)
            messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
        }, 200);
      } catch (error) {
        console.error("Fetch error:", error);
      }
    };
    fetchData();
  }, []);

  useEffect(() => {
    if (messagesRef?.current)
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
  }, []);

  const handleSaveMessage = async (messageId: string, newContent: string) => {
    let updatedMessage = activeModule.messages.find(f=>f.messageId === messageId);

    if(updatedMessage)
    {
      updatedMessage.content = newContent;

      // Update local store immediately (optimistic)
      setActiveModule((prev) => ({
        ...prev,
        messages: (prev.messages ?? []).map((m) =>
          m.messageId === messageId ? { ...m, content: newContent } : m
        ),
      }));

      // Save to backend
      const response = await putToServerApiAsync(`api/chat/${activeModule.chatId}/messages/${messageId}`, updatedMessage);
      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Updating message failed. [${JSON.stringify(serverApiException)}]`);
      }
    }
  };

  const lastMessageIndex = (activeModule?.messages?.length ?? 0) - 1;
  const editableMessageIndex = lastMessageIndex - 3;
  const isSendingMessage = !!activeModule?.mainQueryId; // blocked while AI is replying

  return (
    <main className={styles.chatComponent}>
      <div className={styles.messagesContainer} ref={messagesRef}>
        {activeModule?.messages && activeModule.messages.length > 0 ? (
          activeModule.messages.map((message, index) => (
            <ChatMessageComponent
              key={index}
              messagesRef={messagesRef}
              message={message}
              enableSwipeBtn={index >= editableMessageIndex}
              isEditable={!isSendingMessage && index >= editableMessageIndex}
              onSave={handleSaveMessage}
            />
          ))
        ) : (
          <p />
        )}
        <div className={styles.userInputContainer}>
          <UserInputComponent messagesRef={messagesRef} />
        </div>
      </div>
    </main>
  );
}