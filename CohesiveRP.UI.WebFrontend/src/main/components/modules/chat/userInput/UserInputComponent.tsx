import styles from "./UserInputComponent.module.css";
import { useRef, useState, useEffect  } from "react";
import { HiChip } from "react-icons/hi";
import { BiSolidPaperPlane, BiPaperPlane  } from "react-icons/bi";
import { ImSpinner2 } from "react-icons/im";
import { LuServerOff } from "react-icons/lu";

// Backend webapi
import { getFromServerApiAsync, postToServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";

import { TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY } from "../../../../Constants";

// Store
import { sharedContext } from '../../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../../store/SharedContextChatType";
import type { ChatMessageResponseDto } from "../../../../../ResponsesDto/chat/ChatMessageResponseDto";
import type { BackgroundQueryResponseDto } from "../../../../../ResponsesDto/chat/BackgroundQueryResponseDto";
import { useChatMessages } from "../../../../../store/MessagesStoreContext";
import type { BackgroundQueriesResponseDto } from "../../../../../ResponsesDto/chat/BackgroundQueriesResponseDto";

interface Props {
  messagesRef?: React.RefObject<HTMLDivElement | null>;
}

export default function UserInputComponent({ messagesRef }: Props) {
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const [localInput, setLocalInput] = useState(activeModule?.currentUserInputValue ?? "");
  const debounceTimer = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [messages, setMessages] = useChatMessages(activeModule?.chatId);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const [hoveringSendBtn, setHoveringSendBtn] = useState(false);
  const [isInputBlockedDueToServer, setIsInputBlockedDueToServer] = useState(false);
  const [sendMessageQueryStatus, setSendMessageQueryStatus] = useState("");
  const [networkError, setNetworkError] = useState(false);
  const [isLoadingInitialState, setIsLoadingInitialState] = useState(true);
  const backgroundQueryNetworkError = useRef(0);
  const isStreamingQueryResult:boolean = false;
  const didSentSceneTrackerRefreshToken = useRef(false);

  useEffect(() => {
    if (messages.length <= 0)
      return;

    const lastMessageContent = messages[messages.length - 1].content;
    if (lastMessageContent && lastMessageContent === localInput) {
      setLocalInput("");
      if (activeModule?.chatId) {
        localStorage.setItem(`chatInput_${activeModule.chatId}`, "");
      }

      setActiveModule((prev) => prev ? { ...prev, currentUserInputValue: "" } : prev);
    }
  }, [messages]);

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

  useEffect(() => {
    if (!activeModule?.chatId)
      return;

    const abortController = new AbortController();
    const fetchBackgroundQueries = async () => {

      if(!activeModule?.chatId) {
        console.error(`Couldn't query background queries since there is no tracked chatId.`);
        return;
      }

      const response = await getFromServerApiAsync<BackgroundQueriesResponseDto>(`api/backgroundQueries?chatId=${activeModule?.chatId}`, abortController.signal);

      if (abortController.signal.aborted)
        return;

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code !== 200 || serverApiException?.message) {
        console.error(`Fetching background queries on load failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
        setNetworkError(true);
        return;
      }

      setIsLoadingInitialState(false);
      
      if(!response.queries || response.queries.length <= 0){
        return;
      }

      const mainQuery = response.queries.find(query => query.tags.some(tag => tag === "main"));
      if (mainQuery) {
        setIsInputBlockedDueToServer(true);
        setActiveModule((prev) =>
          prev ? { ...prev, mainQueryId: mainQuery.backgroundQueryId } : prev
        );

        setMessages((prev) => [
          ...prev,
          {
            messageId: TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY,
            content: "...",
            createdAtUtc: null,
            sourceType: 1,
            messageIndex: (activeModule.nbColdMessages ?? 0) + messages.length + 1,
            summarized: false,
            avatarFilePath: "avatar",
            characterId: null,
            characterName: "",
            personaId: null,
            personaName: "",
          },// Add a fake AI message at the bottom. We'll update this message as the generation go and we'll replace that whole message once the generation is done
        ]);
      }
    };

    fetchBackgroundQueries();
    return () => abortController.abort();

  }, [activeModule?.chatId]);

const adjustTextareaHeight = () => {
    const el = textareaRef.current;
  const container = messagesRef?.current;
  if (!el) return;

  // Check if user is at (or near) the bottom BEFORE collapsing the textarea
  const wasAtBottom = container
    ? container.scrollHeight - container.scrollTop - container.clientHeight < 10
    : false;

  el.style.height = "0px";
  const targetHeight = el.scrollHeight;
  const maxHeight = 140;
  const newHeight = Math.min(targetHeight, maxHeight);

  el.style.height = `${newHeight}px`;
  el.style.overflowY = targetHeight > maxHeight ? "auto" : "hidden";

  // Only restore scroll to bottom if they were already there
  if (wasAtBottom && container) {
    container.scrollTop = container.scrollHeight;
  }
  };

  useEffect(() => {
    adjustTextareaHeight();
  }, [localInput]);

  const handleInput = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const value = e.target.value;

    // Update local state immediately — only this component re-renders
    setLocalInput(value);
    UpdateInputControlState();

    // Debounce the expensive side-effects
    if (debounceTimer.current) clearTimeout(debounceTimer.current);
    debounceTimer.current = setTimeout(() => {
      setActiveModule((prev) => prev ? { ...prev, currentUserInputValue: value } : prev);
      if (activeModule?.chatId) {
        localStorage.setItem(`chatInput_${activeModule.chatId}`, value);
      }
    }, 300);
  };

  useEffect(() => {
    // Only run if we have a query to track
    if (!activeModule?.mainQueryId || networkError)
      return;

    setActiveModule((prev) =>
      prev ? { ...prev, sceneTrackerRefreshing: true } : prev
    );

    const pollInterval = setInterval(async () => {
      try {
        if(!activeModule?.mainQueryId) {
          console.error(`The front end didn't have any tracked background query. The backend whereabouts are unknown.`);
          return;
        }

        let response:BackgroundQueryResponseDto | null = await getFromServerApiAsync<BackgroundQueryResponseDto>(`api/backgroundQueries/${activeModule.mainQueryId}`);

        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if(!response || response.code != 200 || serverApiException?.message) {
          console.error(`Fetching Main background query failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);

          if(backgroundQueryNetworkError.current >= 4){
            setSendMessageQueryStatus("");
            setIsInputBlockedDueToServer(false);
            clearInterval(pollInterval);
            setNetworkError(true);
            return;
          }
          
          backgroundQueryNetworkError.current += 1;
          return;
        }

        // If the query is not inProgress, we'll fetch the generated message
        let realMessageFromStorage:ChatMessageResponseDto | null = null;
        if (response?.status === "InProgress" && !didSentSceneTrackerRefreshToken.current) {
          didSentSceneTrackerRefreshToken.current = true;
          setActiveModule((prev) =>
            prev ? { ...prev, sceneTrackerRefreshToken: (prev.sceneTrackerRefreshToken ?? 0) + 1, sceneTrackerRefreshing: false } : prev
          );
        }

        if (response?.status !== "Pending" && response?.status !== "InProgress") {
          if(!response?.chatId) {
            console.error(`The background query to generate AI response was done, but there is no tracked ChatId.`);
            return;
          }

          if(!response.linkedId) {
            console.error(`The background query to generate AI response was done, but the underlying generated message is null.`);
            return;
          }

          realMessageFromStorage = await getFromServerApiAsync<ChatMessageResponseDto>(`api/chat/${response?.chatId}/messages/${response?.linkedId}`);

          let serverApiException = realMessageFromStorage as ServerApiExceptionResponseDto | null;
          if(!response || response.code != 200 || serverApiException?.message) {
            console.error(`Fetching real message from main background query result failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
          }
        }

        setSendMessageQueryStatus(response?.status ?? "");
        setMessages((prev) => {
          const updated = [...prev];
          const tempAIReplyMessageIndex = updated.findIndex(f => f.messageId === TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY);

          // Update the fake AI message to show generation progress
          if (tempAIReplyMessageIndex !== -1) {
            updated[tempAIReplyMessageIndex] = { ...updated[tempAIReplyMessageIndex], content: response?.content ?? "..." };
            if (response?.status !== "InProgress" && response?.status !== "Pending") {
              didSentSceneTrackerRefreshToken.current = false;

              // swap temp id for real id
              if (realMessageFromStorage?.messageObj) {
                let index = updated[tempAIReplyMessageIndex].messageIndex;
                updated[tempAIReplyMessageIndex] = realMessageFromStorage.messageObj;
                updated[tempAIReplyMessageIndex].messageIndex = index;
              } else {
                updated[tempAIReplyMessageIndex].messageId = response?.linkedId ?? "";
              }

              updated[tempAIReplyMessageIndex].startFocusedGenerationDateTimeUtc = response?.startFocusedGenerationDateTimeUtc ?? "";
              updated[tempAIReplyMessageIndex].endFocusedGenerationDateTimeUtc = response?.endFocusedGenerationDateTimeUtc ?? "";
            }
          }
          return updated;
      });

      if (response?.status !== "InProgress" && response?.status !== "Pending") {
        console.log("Generation complete, clearing polling.");
        backgroundQueryNetworkError.current = 0;
        clearInterval(pollInterval);

        // Fetch the updated player message (avatarFilePath is now populated)
        const playerMsgId = activeModule?.lastPlayerMessageId;
        if (playerMsgId && response?.chatId) {
          const playerMsgResponse = await getFromServerApiAsync<ChatMessageResponseDto>(
            `api/chat/${response.chatId}/messages/${playerMsgId}`
          );
          const updatedPlayerMsg = playerMsgResponse?.messageObj;
          if (updatedPlayerMsg) {
            setMessages((prev) =>
              prev.map((m) => m.messageId === playerMsgId ? { ...m, avatarFilePath: updatedPlayerMsg.avatarFilePath } : m)
            );
          }
        }
        
        // These local state updates are now safe because they aren't 
        // nested inside another component's state update logic
        setIsInputBlockedDueToServer(false);
        setSendMessageQueryStatus(response?.status ?? "");
        setActiveModule((prev) => prev ? { ...prev, mainQueryId: null } : prev);
        
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
      //textareaRef.current?.blur();
    } else {
      textareaRef.current?.focus();
    }
  };

  const handleSendPlayerMessage = async () => {
    if (isInputBlockedDueToServer || localInput === undefined){
      return;
    }
    
    console.log(`Sending new message from player to server.`);
    setIsInputBlockedDueToServer(true)
    
    // Fetch from server api
    const payload = {
      content: localInput,
      createdAtUtc: new Date().toUTCString()
    };
    
    let response:ChatMessageResponseDto | null = await postToServerApiAsync<ChatMessageResponseDto>(`api/chat/${activeModule?.chatId}/messages`, payload);

    let serverApiException = response as ServerApiExceptionResponseDto | null;
    if(!response || response.code != 200 || serverApiException?.message){
      console.error(`Sending player message to backend failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);

      // TODO: show err to user
      setSendMessageQueryStatus("");
      setIsInputBlockedDueToServer(false);
      return;
    }

    console.log(`Sending player message to backend succeeded.`);
    setSendMessageQueryStatus("Completed");
    setLocalInput(""); // clear immediately
    localStorage.setItem(`chatInput_${activeModule.chatId}`, "");
    setActiveModule((prev) => prev ? { ...prev, currentUserInputValue: "" } : prev);
    
    // reflect those messages in the UI!
    response.messageObj.messageIndex = (activeModule.nbColdMessages ?? 0) + messages.length + 1;
    const newPlayerMsg = response.messageObj;

    if (!newPlayerMsg) 
      return;

    if(newPlayerMsg.messageId !== null) {
      setMessages((prev) => [
        ...prev,
        newPlayerMsg,
        {
          messageId: TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY,
          content: "...",
          createdAtUtc: null,
          sourceType: 1,
          messageIndex: (activeModule.nbColdMessages ?? 0) + messages.length + 2,
          summarized: false,
          avatarFilePath: null,
          characterId: null,
          characterName: "",
          personaId: null,
          personaName: "",
        },// Add a fake AI message at the bottom. We'll update this message as the generation go and we'll replace that whole message once the generation is done
      ]);
    } else {
      setMessages((prev) => [
        ...prev,
        {
          messageId: TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY,
          content: "...",
          createdAtUtc: null,
          sourceType: 1,
          messageIndex: (activeModule.nbColdMessages ?? 0) + messages.length + 2,
          summarized: false,
          avatarFilePath: null,
          characterId: null,
          characterName: "",
          personaId: null,
          personaName: "",
        },// Add a fake AI message at the bottom. We'll update this message as the generation go and we'll replace that whole message once the generation is done
      ]);

      cleanupMessages();
    }

    setActiveModule((prev) => prev ? {
      ...prev,
      mainQueryId: response.mainQueryId,
      lastPlayerMessageId: response.messageObj?.messageId ?? null,
      currentUserInputValue: "" 
    } : prev);

    UpdateInputControlState();
    // setIsInputBlockedDueToServer(false);

    setTimeout(() => {
      if(messagesRef?.current) {
        messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
      }
    }, 200);
  };

  const cleanupMessages = () => {
    // TODO: keep settings in state from Db and references that
    if(messages.length >= 30){
      // Keep only the 30 most recent messages
      setMessages((prev) =>
        [...prev]
          .sort((a, b) => {
            if (!a.createdAtUtc)
              return 1;
            
            if (!b.createdAtUtc)
              return -1;

            return new Date(a.createdAtUtc).getTime() - new Date(b.createdAtUtc).getTime();
          })
          .slice(-30)
      );
    }
  };

  const handleCancelLatestPlayerMessage = () => {
    // optional: cancel request / noop / show tooltip
    console.log("Cancelling... TODO (not implemented)");
    // setIsInputBlockedDueToServer(false);
    // setSendMessageQueryStatus("");

    // TODO: cancel and then setIsInputBlockedDueToServer(false)
  };

  return (
    <main className={styles.userInputComponent}>
      <div className={styles.inputContainer}>
        <HiChip className={styles.autoCorrectIcon} />
        <div className={styles.inputAutoCorrectSeparator} />
        <div className={styles.inputControlContainer}>
          <textarea className={styles.inputControl} rows={1} ref={textareaRef} onChange={handleInput} value={localInput} placeholder="Type a message..."/>
        </div>
        <div className={styles.inputSendSeparator} />
          <div
          className={styles.rightInputControlContainer}
          onMouseEnter={() => setHoveringSendBtn(true)}
          onMouseLeave={() => setHoveringSendBtn(false)}
          onClick={
            networkError || isLoadingInitialState ? undefined : 
            isInputBlockedDueToServer
              ? handleCancelLatestPlayerMessage
              : handleSendPlayerMessage
          }>
            {networkError ? (
              <LuServerOff />
            ) : (
              isInputBlockedDueToServer || isLoadingInitialState ? (
              <ImSpinner2 className={sendMessageQueryStatus === "" ? styles.sendInputSpinnerWaitingServerAck : (sendMessageQueryStatus === "Pending" ? styles.sendInputSpinnerWaitingMessagePending : ((sendMessageQueryStatus === "InProgress" ? styles.sendInputSpinnerWaitingMessageProcess : styles.sendInputSpinnerWaitingMessageDefault))) } />
              ) : hoveringSendBtn ? (
                <BiPaperPlane className={styles.sendInputIcon} />
              ) : (
                <BiSolidPaperPlane className={styles.sendInputIcon} />
              )
            )}
        </div>
      </div>
    </main>
  );
}