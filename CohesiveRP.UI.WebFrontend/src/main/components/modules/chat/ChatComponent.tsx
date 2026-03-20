import styles from "./ChatComponent.module.css";
import { Fragment, useEffect, useRef } from "react";
import ChatMessageComponent from "./message/ChatMessageComponent";
import UserInputComponent from "./userInput/UserInputComponent";
import { deleteFromServerApiAsync, getFromServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ChatMessagesResponseDto } from "../../../../ResponsesDto/chat/ChatMessagesResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../store/SharedContextChatType";
import { useChatMessages } from "../../../../store/MessagesStoreContext";
import SceneTrackerComponent from "./sceneTracker/SceneTrackerComponent";

export default function ChatComponent() {
  const messagesRef = useRef<HTMLDivElement>(null);
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
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
        setActiveModule((prev) => prev ? { ...prev, nbColdMessages: response.nbColdMessages } : prev);
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
    updatedMessage.content = newContent;
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

  //const isSendingMessage = !!activeModule?.mainQueryId; // blocked while AI is replying

  return (
    <main className={styles.chatComponent}>
      <div className={styles.messagesContainer} ref={messagesRef}>
        {messages.length > 0 ? (
          messages.map((message, index) => {
            const isLastMessage = index === messages.length - 1;

            return (
              <Fragment key={message.messageId}>
                {messages.length > 1 && isLastMessage && <SceneTrackerComponent />} 

                <ChatMessageComponent
                  message={message}
                  chatId={activeModule?.chatId}
                  enableDeleteBtn={isLastMessage} 
                  enableSwipeBtn={isLastMessage}
                  isEditable={!message.summarized && index >= messages.length - 3}
                  onSave={handleSaveMessage}
                  onDelete={handleDeleteMessage}
                />
              </Fragment>
            );
          })
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