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
}

export default function UserInputComponent({ messagesRef }: Props) {
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const [hoveringSendBtn, setHoveringSendBtn] = useState(false);
  const [playerMessage, setPlayerMessage] = useState("");
  const [isInputBlockedDueToServer, setIsInputBlockedDueToServer] = useState(false);
  const [isSendingMessageToServer, setIsSendingMessageToServer] = useState(false);
  const [isWaitingOnPlayerMessageServerProcess, setIsWaitingOnPlayerMessageServerProcess] = useState(false);
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
    const element = textareaRef.current;
    if (!element)
      return;

    element.style.height = "auto";
    const targetHeight = element.scrollHeight;
    element.style.height = `${Math.min(targetHeight, 140)}px`;
    element.style.overflowY = targetHeight > 140 ? "auto" : "hidden";

    if (messagesRef?.current) {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
  };

  useEffect(() => {
    adjustTextareaHeight();
  }, [playerMessage]);

  const handleInput = () => {
    const el = textareaRef.current;
    if (!el) {
      return;
    }

    el.style.marginBottom = "-0.3em";// hack...sigh
    el.style.height = "auto";
    el.style.height = Math.min(el.scrollHeight, 140) + "px";
    el.style.overflowY = el.scrollHeight > 140 ? "auto" : "hidden";

    // Scroll parent messages container to bottom
    if (messagesRef?.current) {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
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
      if (response?.status !== "InProgress") {
        realMessageFromStorage = await getFromServerApiAsync<ChatMessageResponseDto>(`api/chat/${response?.chatId}/messages/${response?.linkedId}`);

        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if(!response || response.code != 200 || serverApiException?.message) {
          console.error(`Fetching real message from main background query result failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
        }
      }

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
        if(response != null && response.status !== "InProgress") {
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

          // Query is done
          return { ...prev, messages: updatedMessages, mainQueryId: null };
        }

        // Still inProgress
        return { ...prev, messages: updatedMessages };
      });

      if (response?.status !== "InProgress") {
        console.log("Generation complete, clearing polling.");
        clearInterval(pollInterval);
        
        // These local state updates are now safe because they aren't 
        // nested inside another component's state update logic
        setIsSendingMessageToServer(false);
        setIsWaitingOnPlayerMessageServerProcess(false);
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

  const handleSendPlayerMessage = async () => {
    if (isSendingMessageToServer || !playerMessage || !playerMessage.trim()){
      return;
    }
    
    console.log(`Sending new message from player to server.`);
    setIsInputBlockedDueToServer(true)
    setIsSendingMessageToServer(true);
    
    // Fetch from server api
    const payload = {
      content: playerMessage,
      timestampUtc: new Date().toUTCString()
    };
    
    let response:ChatMessageResponseDto | null = await postToServerApiAsync<ChatMessageResponseDto>(`api/chat/${activeModule?.chatId}/messages`, payload);

    let serverApiException = response as ServerApiExceptionResponseDto | null;
    if(!response || response.code != 200 || serverApiException?.message){
      console.error(`Sending player message to backend failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);

      // TODO: show err to user
      setIsWaitingOnPlayerMessageServerProcess(false);
      setIsSendingMessageToServer(false);
      setIsInputBlockedDueToServer(false);
      return;
    }

    console.log(`Sending player message to backend succeeded.`);
    setIsWaitingOnPlayerMessageServerProcess(true);
    setPlayerMessage(""); // clear input on success
    
    // reflect those messages in the UI!
    response.messageObj.messageIndex = activeModule.messages.length + 1;
    setActiveModule((prev) => ({
      ...prev,
      messages: [...(prev.messages || []),// Keep messages history
      response.messageObj,// Add new player message at the bottom
      {
        messageId: TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY, content: "...", createdAtUtc: null, sourceType: 1, messageIndex: prev.messages.length + 2, summarized: false }],// Add a fake AI message at the bottom. We'll update this message as the generation go and we'll replace that whole message once the generation is done
        mainQueryId: response.mainQueryId,// Track the main query id to know the status of the AI reply
    }));

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
    setIsWaitingOnPlayerMessageServerProcess(false);

    // TODO: cancel and then setIsInputBlockedDueToServer(false)
  };

  return (
    <main className={styles.userInputComponent}>
      <div className={styles.inputContainer}>
        <HiChip className={styles.autoCorrectIcon} />
        <div className={styles.inputAutoCorrectSeparator} />
        <div className={styles.inputControlContainer}>
          <textarea className={styles.inputControl} rows={1} ref={textareaRef} onInput={handleInput} onChange={(e) => setPlayerMessage(e.target.value)} value={playerMessage} placeholder="Type a message..."/>
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
              <ImSpinner2 className={isWaitingOnPlayerMessageServerProcess ? styles.sendInputSpinnerWaitingMessageProcess : styles.sendInputSpinnerWaitingServerAck} />
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