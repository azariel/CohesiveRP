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
//import type { ChatResponseDto } from "../../../../ResponsesDto/chat/ChatResponseDto";

export default function ChatComponent() {
  const messagesRef = useRef<HTMLDivElement>(null);
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const didComponentMountAlready = useRef(false);

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;

    didComponentMountAlready.current = true;

    // const fetchChat = async () => {
    //   try {
    //     let chatModule = activeModule as SharedContextChatType;
    //     const responseChat: ChatResponseDto | null = await getFromServerApiAsync<ChatResponseDto>(`api/chats/${chatModule.chatId}`);
        
    //     let serverApiException = responseChat as ServerApiExceptionResponseDto | null;
    //     if (!responseChat || responseChat.code != 200 || serverApiException?.message) {
    //       console.error(`Call to fetch specific chat failed. [${JSON.stringify(serverApiException)}]`);
    //       return;
    //     }

    //     defaultChatAvatarId.current = responseChat.avatarCharacterId;
    //     console.log(`Specific chat fetched successfully.`);
    //   } catch (error) {
    //     console.error("Fetch chat error:", error);
    //   }
    // };

    const fetchData = async () => {
      try {
        let chatModule = activeModule as SharedContextChatType;
        
        if(!chatModule?.chatId) {
          console.error(`Couldn't load chat. ChatId was undefined.`);
          return;
        }

        const response: ChatMessagesResponseDto | null = await getFromServerApiAsync<ChatMessagesResponseDto>(`api/chat/${chatModule.chatId}/messages/hot`);
        
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch specific chat messages failed. [${JSON.stringify(serverApiException)}]`);
          return;
        }

        setActiveModule({ ...activeModule, messages: response.messages ?? [] });
        console.log(`Specific chat messages fetched successfully.`);
        setTimeout(() => {
          if (messagesRef?.current)
            messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
        }, 200);
      } catch (error) {
        console.error("Fetch messages error:", error);
      }
    };

    //fetchChat();
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

  const handleDeleteMessage = async (messageId: string) => {
    // Optimistic removal from local store
    setActiveModule((prev) => ({
      ...prev,
      messages: (prev.messages ?? []).filter((m) => m.messageId !== messageId),
    }));

    // Delete on backend
    const response = await deleteFromServerApiAsync(`api/chat/${activeModule.chatId}/messages/${messageId}`);
    const serverApiException = response as ServerApiExceptionResponseDto | null;
    if (!response || serverApiException?.message) {
      console.error(`Deleting message failed. [${JSON.stringify(serverApiException)}]`);

      // Optional: roll back optimistic removal on failure
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
              defaultChatAvatarId={activeModule.defaultChatAvatar ?? ""}
              enableDeleteBtn={index >= activeModule.messages.length-1}
              enableSwipeBtn={index >= activeModule.messages.length-1}
              isEditable={!isSendingMessage && index >= editableMessageIndex}
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