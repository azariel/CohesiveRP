import styles from "./ChatComponent.module.css";
import { Fragment, useEffect, useRef, useState } from "react";
import ChatMessageComponent from "./message/ChatMessageComponent";
import UserInputComponent from "./userInput/UserInputComponent";
import { deleteFromServerApiAsync, getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ChatMessagesResponseDto } from "../../../../ResponsesDto/chat/ChatMessagesResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../store/SharedContextChatType";
import { useChatMessages } from "../../../../store/MessagesStoreContext";
import SceneTrackerComponent from "./sceneTracker/SceneTrackerComponent";
import ChatRollsComponent from "./chatRolls/ChatRollsComponent";
import InteractiveUserInputComponent from "./interactiveUserInput/InteractiveUserInputComponent";
import MobileAvatarBannerComponent from "./mobileAvatarBanner/MobileAvatarBannerComponent";
import CharacterSheetInstancesSelectionComponent from "./characterSheetInstancesSelection/CharacterSheetInstancesSelectionComponent";
import CharacterSheetInstanceComponent from "./characterSheetInstances/CharacterSheetInstanceComponent";

import type { ChatMessageResponseDto } from "../../../../ResponsesDto/chat/ChatMessageResponseDto";
import { TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY } from "../../../Constants";

export default function ChatComponent() {
  const messagesRef = useRef<HTMLDivElement>(null);
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const [messages, setMessages] = useChatMessages(activeModule?.chatId);
  const didComponentMountAlready = useRef(false);

  /* ── Which character's instance is open, when the header has switched us into
     the character-sheets sub-view. Local state: doesn't need to survive
     leaving the chat, and resets whenever the sub-view itself changes. ── */
  const [selectedInstanceCharacter, setSelectedInstanceCharacter] = useState<{ characterId: string; characterName: string } | null>(null);

  useEffect(() => {
    if (activeModule?.chatSubView !== "characterSheetInstances") {
      setSelectedInstanceCharacter(null);
    }
  }, [activeModule?.chatSubView]);

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;

    didComponentMountAlready.current = true;

    setActiveModule((prev) => prev ? { ...prev, latestPlayerDescription: undefined } : prev);

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
        setActiveModule((prev) => prev ? { ...prev, nbColdMessages: response.nbColdMessages, hotMessagesLoaded: true, latestPlayerDescription: undefined } : prev);

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
    {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
  }, []);

  const handleSaveMessage = async (messageId: string, newContent: string) => {
    const updatedMessage = messages.find((m) => m.messageId === messageId);
    if (!updatedMessage)
      return;
  
    const payload = { ...updatedMessage, content: newContent };

    setMessages((prev) => prev.map((m) => m.messageId === messageId ? payload : m));

    updatedMessage.content = newContent;
    const response = await putToServerApiAsync(`api/chat/${activeModule.chatId}/messages/${messageId}`, updatedMessage);
    const serverApiException = response as ServerApiExceptionResponseDto | null;
    if (!response || serverApiException?.message) {
      console.error(`Updating message failed. [${JSON.stringify(serverApiException)}]`);
    }
  };

  const handleSwipeMessage = async (chatId: string, messageId: string) => {
  const targetMessage = messages.find((m) => m.messageId === messageId);
  if (!targetMessage)
    return;

  const payload = { chatId, messageId };
  const response = await postToServerApiAsync<ChatMessageResponseDto>(`api/chat/${activeModule.chatId}/messages/${messageId}/swipe`, payload);

  const serverApiException = response as ServerApiExceptionResponseDto | null;
  if (!response || response.code !== 200 || serverApiException?.message) {
    console.error(`Swiping message failed. [${JSON.stringify(serverApiException)}]`);
    return;
  }

  setMessages((prev) =>
    prev.map((m) =>
      m.messageId === messageId
        ? {
            messageId: TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY,
            content: "...",
            thinkingContent: "",
            createdAtUtc: null,
            sourceType: 1,
            messageIndex: targetMessage.messageIndex,
            summarized: false,
            characterAvatars: [],
            characterId: null,
            characterName: "",
            personaId: null,
            personaName: "",
          }
        : m
    )
  );

  setActiveModule((prev) =>
    prev ? { ...prev, mainQueryId: response.mainQueryId, latestPlayerDescription: undefined } : prev
  );
};

  const handleDeleteMessage = async (messageId: string) => {
    const response = await deleteFromServerApiAsync(`api/chat/${activeModule.chatId}/messages/${messageId}`);
    const serverApiException = response as ServerApiExceptionResponseDto | null;
    if (!response || serverApiException?.message) {
      console.error(`Deleting message failed. [${JSON.stringify(serverApiException)}]`);
    } else {
      setMessages((prev) => prev.filter((m) => m.messageId !== messageId));
    }
  };

  return (
    <main className={styles.chatComponent}>
      {activeModule?.chatSubView === "characterSheetInstances" && activeModule?.chatId ? (
        <div className={styles.chatSubViewContainer}>
          {selectedInstanceCharacter ? (
            <CharacterSheetInstanceComponent
              characterId={selectedInstanceCharacter.characterId}
              chatId={activeModule.chatId}
              characterName={selectedInstanceCharacter.characterName}
              onBack={() => setSelectedInstanceCharacter(null)}
            />
          ) : (
            <CharacterSheetInstancesSelectionComponent
              chatId={activeModule.chatId}
              onSelectCharacter={(characterId, characterName) =>
                setSelectedInstanceCharacter({ characterId, characterName })
              }
            />
          )}
        </div>
      ) : (
        <div className={styles.messagesContainer} ref={messagesRef}>
          {messages.length > 0 ? (
            messages.map((message, index) => {
              const isLastMessage = index === messages.length - 1;

              return (
                <Fragment key={message.messageId}>
                  {messages.length > 1 && isLastMessage && (
                    <>
                      <MobileAvatarBannerComponent />
                      <SceneTrackerComponent />
                    </>
                  )}

                  <ChatMessageComponent
                    message={message}
                    chatId={activeModule?.chatId}
                    isLastMessage={isLastMessage}
                    enableDeleteBtn={isLastMessage && message.messageId !== TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY}
                    enableSwipeBtn={isLastMessage && message.messageId !== TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY}
                    isEditable={!message.summarized && index >= messages.length - 3}
                    onSave={handleSaveMessage}
                    onSwipe={handleSwipeMessage}
                    onDelete={handleDeleteMessage}
                  />
                </Fragment>
              );
            })
          ) : (
            <p />
          )}
          {activeModule?.chatId ? (
          <div className={styles.userInputContainer}>
            <InteractiveUserInputComponent
              chatId={activeModule.chatId}
              refreshToken={activeModule?.interactiveInputRefreshToken}
            />
            <ChatRollsComponent sceneTrackerRefreshToken={activeModule?.sceneTrackerRefreshToken} />
            <UserInputComponent messagesRef={messagesRef} />
          </div>
          ):(
            <p>Chat not found.</p>
          )}
        </div>
      )}
    </main>
  );
}