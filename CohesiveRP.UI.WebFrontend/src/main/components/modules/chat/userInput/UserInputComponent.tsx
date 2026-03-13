import styles from "./UserInputComponent.module.css";
import { useRef, useState, useEffect  } from "react";
import { HiChip } from "react-icons/hi";
import { BiSolidPaperPlane, BiPaperPlane  } from "react-icons/bi";
import { ImSpinner2 } from "react-icons/im";

// Backend webapi
import { getFromServerApiAsync, postToServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";

import { TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY } from "../../../../Constants";

// Store
import { sharedContext } from '../../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../../store/SharedContextChatType";
import type { ChatMessageResponseDto } from "../../../../../ResponsesDto/chat/ChatMessageResponseDto";
import type { BackgroundQueryResponseDto } from "../../../../../ResponsesDto/chat/BackgroundQueryResponseDto";

interface Props {
  messagesRef?: React.RefObject<HTMLDivElement | null>;
  defaultChatAvatarId: string | null;
}

export default function UserInputComponent({ messagesRef, defaultChatAvatarId }: Props) {
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const [hoveringSendBtn, setHoveringSendBtn] = useState(false);
  const [isInputBlockedDueToServer, setIsInputBlockedDueToServer] = useState(false);
  const [isSendingMessageToServer, setIsSendingMessageToServer] = useState(false);
  const [sendMessageQueryStatus, setSendMessageQueryStatus] = useState("");
  const isStreamingQueryResult:boolean = false;

  useEffect(() => {
    const textarea = textareaRef.current;
    const messagesContainer = messagesRef?.current;

    if (!textarea || !messagesContainer)
      return;

    const handleFocus = () => {
      setTimeout(() => {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
      }, 300); // 300ms is usually enough for keyboard animation
    };

    textarea.addEventListener("focus", handleFocus);

    return () => {
      textarea.removeEventListener("focus", handleFocus);
    };
  }, [messagesRef]);

const adjustTextareaHeight = () => {
    const el = textareaRef.current;
    if (!el) return;

    // 1. Reset height to 'auto' first so it can shrink if the user deletes text
    el.style.height = "auto";
    
    // 2. Calculate the new height based on the content
    const targetHeight = el.scrollHeight;
    const maxHeight = 140;

    // 3. Apply the constrained height and toggle the scrollbar
    el.style.height = `${Math.min(targetHeight, maxHeight)}px`;
    el.style.overflowY = targetHeight > maxHeight ? "auto" : "hidden";

    // 4. Keep the main message window scrolled to the bottom
    if (messagesRef?.current) {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
  };

  useEffect(() => {
    adjustTextareaHeight();
  }, [activeModule]);

const handleInput = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const value = e.target.value;
    setActiveModule((prev) => prev ? { ...prev, currentUserInputValue: value } : prev);
    
    // Save per chatId
    if (activeModule?.chatId) {
      localStorage.setItem(`chatInput_${activeModule.chatId}`, value);
    }
    
    adjustTextareaHeight();
  };

  useEffect(() => {
  // Only run if we have a query to track
  if (!activeModule?.mainQueryId)
    return;

  const pollInterval = setInterval(async () => {
    try {
      let response:BackgroundQueryResponseDto | null = await getFromServerApiAsync<BackgroundQueryResponseDto>(`api/backgroundQueries/${activeModule?.mainQueryId}`);

      let serverApiException = response as ServerApiExceptionResponseDto | null;
      if(!response || response.code != 200 || serverApiException?.message) {
        console.error(`Fetching Main background query failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
      }

      // If the query is not inProgress, we'll fetch the generated message
      let realMessageFromStorage:ChatMessageResponseDto | null = null;
      if (response?.status !== "Pending" && response?.status !== "InProgress") {
        realMessageFromStorage = await getFromServerApiAsync<ChatMessageResponseDto>(`api/chat/${response?.chatId}/messages/${response?.linkedId}`);

        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if(!response || response.code != 200 || serverApiException?.message) {
          console.error(`Fetching real message from main background query result failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
        }
      }

      setSendMessageQueryStatus(response?.status ?? "");

      // TODO: if backend is closed whilst we're waiting here, we want to handle this cleanly!

      setActiveModule((prev) => {
        if (!prev)
          return prev;

        const updatedMessages = [...(prev.messages || [])];
        const tempAIReplyMessageIndex = updatedMessages.findIndex(f => f.messageId === TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY);

        // Update the fake AI message to show generation progress
        if (tempAIReplyMessageIndex !== -1) {
          updatedMessages[tempAIReplyMessageIndex] = {
            ...updatedMessages[tempAIReplyMessageIndex],
            content: response?.content ?? "...",
          };
        }

        // If the AI reply generation is done, update states
        if(response != null && response.status !== "InProgress" && response.status !== "Pending") {
          if (tempAIReplyMessageIndex !== -1) {

            if(realMessageFromStorage && realMessageFromStorage.messageObj) {
              updatedMessages[tempAIReplyMessageIndex].messageId = realMessageFromStorage.messageObj.messageId;
              updatedMessages[tempAIReplyMessageIndex].createdAtUtc = realMessageFromStorage.messageObj.createdAtUtc;
              updatedMessages[tempAIReplyMessageIndex].content = realMessageFromStorage.messageObj.content;
              updatedMessages[tempAIReplyMessageIndex].sourceType = realMessageFromStorage.messageObj.sourceType;
            } else {
              updatedMessages[tempAIReplyMessageIndex].messageId = response.linkedId;
              console.error(`Background main query was done, but the underlying message couldn't be retrieved from backend! Impersonating the message with the right id [${response.linkedId}] now, but state is finicky.`);
            }
          }

          UpdateInputControlState();

          // Query is done
          return { ...prev, messages: updatedMessages, mainQueryId: null };
        }

        // Still inProgress
        return { ...prev, messages: updatedMessages };
      });

      if (response?.status !== "InProgress" && response?.status !== "Pending") {
        console.log("Generation complete, clearing polling.");
        clearInterval(pollInterval);
        
        // These local state updates are now safe because they aren't 
        // nested inside another component's state update logic
        setIsSendingMessageToServer(false);
        setSendMessageQueryStatus(response?.status ?? "");
        setIsInputBlockedDueToServer(false);

        if (messagesRef?.current) {
          setTimeout(() => {
            if(messagesRef?.current) {
              messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
            }
          }, 200);
        }
      }

    } catch (err) {
      console.error("Polling main background query error:", err);
      clearInterval(pollInterval);
    }
  }, isStreamingQueryResult ? 1000 : 3000);

  // cleanup
  return () => clearInterval(pollInterval);
}, [activeModule?.mainQueryId, setActiveModule]); // Only re-run if the ID changes

  const UpdateInputControlState = async () => {
    const isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    if (isMobile) {
      textareaRef.current?.blur();
    } else {
      textareaRef.current?.focus();
    }
  };

  const handleSendPlayerMessage = async () => {
    if (isSendingMessageToServer || !activeModule?.currentUserInputValue){
      return;
    }
    
    console.log(`Sending new message from player to server.`);
    setIsInputBlockedDueToServer(true)
    setIsSendingMessageToServer(true);
    
    // Fetch from server api
    const payload = {
      content: activeModule?.currentUserInputValue,
      createdAtUtc: new Date().toUTCString()
    };
    
    let response:ChatMessageResponseDto | null = await postToServerApiAsync<ChatMessageResponseDto>(`api/chat/${activeModule?.chatId}/messages`, payload);

    let serverApiException = response as ServerApiExceptionResponseDto | null;
    if(!response || response.code != 200 || serverApiException?.message){
      console.error(`Sending player message to backend failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);

      // TODO: show err to user
      setSendMessageQueryStatus("");
      setIsSendingMessageToServer(false);
      setIsInputBlockedDueToServer(false);
      return;
    }

    console.log(`Sending player message to backend succeeded.`);
    setSendMessageQueryStatus("Completed");
    setActiveModule((prev) => prev ? { ...prev, currentUserInputValue: "" } : prev); // clear input on success
    
    // reflect those messages in the UI!
    response.messageObj.messageIndex = activeModule.messages.length + 1;
    const newPlayerMsg = response.messageObj;

    if (!newPlayerMsg) 
      return;

    setActiveModule((prev) => {
      if (!prev)
        return prev;

      const currentMessages = prev.messages ?? [];
      return {
        ...prev,
        messages: [
          ...currentMessages,// Keep messages history
        newPlayerMsg,// Add new player message at the bottom
        {
          messageId: TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY,
          content: "...", 
          createdAtUtc: null,
          sourceType: 1,
          messageIndex: prev.messages.length + 2,
          summarized: false,
          avatarId: defaultChatAvatarId,
          characterId: null
        }],// Add a fake AI message at the bottom. We'll update this message as the generation go and we'll replace that whole message once the generation is done
        mainQueryId: response.mainQueryId,// Track the main query id to know the status of the AI reply
      }
    });

    UpdateInputControlState();

    setTimeout(() => {
      if(messagesRef?.current) {
        messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
      }
    }, 200);
  };

  const handleCancelLatestPlayerMessage = () => {
    // optional: cancel request / noop / show tooltip
    console.log("Cancelling... TODO (not implemented)");
    setIsSendingMessageToServer(false);
    setSendMessageQueryStatus("");

    // TODO: cancel and then setIsInputBlockedDueToServer(false)
  };

  return (
    <main className={styles.userInputComponent}>
      <div className={styles.inputContainer}>
        <HiChip className={styles.autoCorrectIcon} />
        <div className={styles.inputAutoCorrectSeparator} />
        <div className={styles.inputControlContainer}>
          <textarea className={styles.inputControl} rows={1} ref={textareaRef} onChange={handleInput} value={activeModule?.currentUserInputValue} placeholder="Type a message..."/>
        </div>
        <div className={styles.inputSendSeparator} />
          <div
          className={styles.rightInputControlContainer}
          onMouseEnter={() => setHoveringSendBtn(true)}
          onMouseLeave={() => setHoveringSendBtn(false)}
          onClick={
            isSendingMessageToServer
              ? handleCancelLatestPlayerMessage
              : handleSendPlayerMessage
          }>
            {isInputBlockedDueToServer ? (
              <ImSpinner2 className={sendMessageQueryStatus === "" ? styles.sendInputSpinnerWaitingServerAck : (sendMessageQueryStatus === "Pending" ? styles.sendInputSpinnerWaitingMessagePending : ((sendMessageQueryStatus === "InProgress" ? styles.sendInputSpinnerWaitingMessageProcess : styles.sendInputSpinnerWaitingMessageDefault))) } />
            ) : hoveringSendBtn ? (
              <BiPaperPlane className={styles.sendInputIcon} />
            ) : (
              <BiSolidPaperPlane className={styles.sendInputIcon} />
            )}
        </div>
      </div>
    </main>
  );
}