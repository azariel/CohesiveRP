import styles from "./ChatComponent.module.css";
import { useEffect, useRef  } from "react";
import ChatMessageComponent from "./message/ChatMessageComponent";
import UserInputComponent from "./userInput/UserInputComponent";

import { getFromServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
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
        //const response:ChatResponseDto | null = await getFromServerApiAsync<ChatResponseDto>(`api/chat/${chatModule.chatId}`);
        const response:ChatMessagesResponseDto | null = await getFromServerApiAsync<ChatMessagesResponseDto>(`api/chat/${chatModule.chatId}/messages/hot`);

        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if(!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch specific chat failed. [${JSON.stringify(serverApiException)}]`);
          return;
        }

        setActiveModule({...activeModule, messages: response.messages ?? []});
        console.log(`Specific chat fetched successfully.`);
      } catch (error) {
        console.error("Fetch error:", error);
      }
    };

    fetchData();
  }, []);

  // Scroll to bottom when component mounts
  useEffect(() => {
    if (messagesRef.current) {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
  }, []);

  return (
    <main className={styles.chatComponent}>
      <div className={styles.messagesContainer} ref={messagesRef}>
        {activeModule?.messages && activeModule.messages.length > 0 ? (
          activeModule.messages.map((message, index) => (
            <ChatMessageComponent key={index} messageContent={message} enableSwipeBtn={index === (activeModule.messages?.length ?? 0) - 1} />
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