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
import { useChatMessages } from "../../../../../store/MessagesStoreContext";
import type { BackgroundQuery } from "../../../../../ResponsesDto/chat/BusinessObjects/BackgroundQuery";
import type { BackgroundQueryResponseDto } from "../../../../../ResponsesDto/chat/BackgroundQueryResponseDto";

interface Props {
  messagesRef?: React.RefObject<HTMLDivElement | null>;
  backgroundQueries: BackgroundQuery[];
  backgroundQueriesLoadingInitial: boolean;
  backgroundQueriesNetworkError: boolean;
  triggerBackgroundQueriesPoll: () => void;
}

export default function UserInputComponent({ messagesRef, backgroundQueries, backgroundQueriesLoadingInitial, backgroundQueriesNetworkError, triggerBackgroundQueriesPoll }: Props) {
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const [localInput, setLocalInput] = useState(activeModule?.currentUserInputValue ?? "");
  const debounceTimer = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [messages, setMessages] = useChatMessages(activeModule?.chatId);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const [hoveringSendBtn, setHoveringSendBtn] = useState(false);
  const [isInputBlockedDueToServer, setIsInputBlockedDueToServer] = useState(false);
  const [sendMessageQueryStatus, setSendMessageQueryStatus] = useState("");
  const [networkError] = useState(false);
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

      setActiveModule((prev) => prev ? { ...prev, currentUserInputValue: "", lastPlayerMessageId: "" } : prev);
    }
  }, [messages]);

  const resumedTrackingRef = useRef(false);

  useEffect(() => {
    if (resumedTrackingRef.current || !activeModule?.hotMessagesLoaded || backgroundQueries.length <= 0)
      return;

    const mainQuery = backgroundQueries.find((q) => q.tags.some((t) => t === "main"));
    if (!mainQuery)
      return;

    resumedTrackingRef.current = true;
    setIsInputBlockedDueToServer(true);
    setActiveModule((prev) => (prev ? { ...prev, mainQueryId: mainQuery.backgroundQueryId } : prev));

    setMessages((prev) => {
      if (prev.some((m) => m.messageId === TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY))
        return prev;

      return [
        ...prev,
        {
          messageId: TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY,
          content: mainQuery.content || "...",
          thinkingContent: "",
          createdAtUtc: null,
          sourceType: 1,
          messageIndex: (activeModule.nbColdMessages ?? 0) + prev.length + 1,
          summarized: false,
          characterAvatars: [],
          characterId: null,
          characterName: "",
          personaId: null,
          personaName: "",
        },
      ];
    });
  }, [backgroundQueries, activeModule?.hotMessagesLoaded]);

  useEffect(() => {
    if (!activeModule?.mainQueryId)
      return;

    const trackedId = activeModule.mainQueryId;
    let cancelled = false;

    const applyUpdate = async (
      status: string,
      content: string | undefined,
      linkedId: string | undefined,
      startFocused: string | undefined,
      endFocused: string | undefined
    ) => {
      if (status === "InProgress" && !didSentSceneTrackerRefreshToken.current) {
        didSentSceneTrackerRefreshToken.current = true;
        setActiveModule((prev) =>
          prev ? { ...prev, sceneTrackerRefreshToken: (prev.sceneTrackerRefreshToken ?? 0) + 1, sceneTrackerRefreshing: false } : prev
        );
      }

      setSendMessageQueryStatus(status);
      setMessages((prev) => {
        const updated = [...prev];
        const idx = updated.findIndex((m) => m.messageId === TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY);
        if (idx !== -1) {
          updated[idx] = { ...updated[idx], content: content ?? "..." };
        }
        return updated;
      });

      const isTerminal = status === "Completed" || status === "Error";
      if (!isTerminal)
        return;

      // Terminal status -> resolve the real message and clean up.
      didSentSceneTrackerRefreshToken.current = false;

      let realMessageFromStorage: ChatMessageResponseDto | null = null;
      if (linkedId) {
        realMessageFromStorage = await getFromServerApiAsync<ChatMessageResponseDto>(
          `api/chat/${activeModule.chatId}/messages/${linkedId}`
        );
        const err = realMessageFromStorage as ServerApiExceptionResponseDto | null;
        if (!realMessageFromStorage || realMessageFromStorage.code != 200 || err?.message) {
          console.error(`Fetching real message from main background query result failed. Code:[${realMessageFromStorage?.code}], Message:[${err?.message}].`);
        }
      }

      if (cancelled) return;

      setMessages((prev) => {
        const updated = [...prev];
        const idx = updated.findIndex((m) => m.messageId === TEMP_AI_REPLY_MESSAGE_ID_WHEN_GENERATING_MAIN_QUERY);
        if (idx !== -1) {
          if (realMessageFromStorage?.messageObj) {
            const index = updated[idx].messageIndex;
            updated[idx] = realMessageFromStorage.messageObj;
            updated[idx].messageIndex = index;
          } else {
            updated[idx].messageId = linkedId ?? "";
          }
          updated[idx].startFocusedGenerationDateTimeUtc = startFocused ?? "";
          updated[idx].endFocusedGenerationDateTimeUtc = endFocused ?? "";
        }
        return updated;
      });

      const playerMsgId = activeModule?.lastPlayerMessageId;
      if (playerMsgId && activeModule?.chatId) {
        const playerMsgResponse = await getFromServerApiAsync<ChatMessageResponseDto>(
          `api/chat/${activeModule.chatId}/messages/${playerMsgId}`
        );
        if (cancelled) return;
        const updatedPlayerMsg = playerMsgResponse?.messageObj;
        if (updatedPlayerMsg) {
          setMessages((prev) =>
            prev.map((m) => (m.messageId === playerMsgId ? { ...m, characterAvatars: updatedPlayerMsg.characterAvatars } : m))
          );
        }
      }

      setIsInputBlockedDueToServer(false);
      setSendMessageQueryStatus(status);
      setActiveModule((prev) =>
        prev
          ? {
              ...prev,
              mainQueryId: null,
              interactiveInputRefreshToken: (prev.interactiveInputRefreshToken ?? 0) + 1,
              sceneTrackerRefreshToken: (prev.sceneTrackerRefreshToken ?? 0) + 1,
            }
          : prev
      );

      if (messagesRef?.current) {
        setTimeout(() => {
          if (messagesRef?.current) messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
        }, 200);
      }
    };

    const mainQuery = backgroundQueries.find((q) => q.backgroundQueryId === trackedId);

    if (mainQuery) {
      applyUpdate(mainQuery.status, mainQuery.content, mainQuery.linkedId, mainQuery.startFocusedGenerationDateTimeUtc, mainQuery.endFocusedGenerationDateTimeUtc);
    } else {
      // Not in the latest shared snapshot — either the poller hasn't caught up yet,
      // or the query finished and dropped out of the active list. Ask directly.
      (async () => {
        const response = await getFromServerApiAsync<BackgroundQueryResponseDto>(`api/backgroundQueries/${trackedId}`);
        if (cancelled || activeModule.mainQueryId !== trackedId)
          return;

        const err = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || err?.message) {
          console.error(`Fetching main background query directly failed. Code:[${response?.code}], Message:[${err?.message}].`);
          return;
        }

        applyUpdate(response.status, response.content, response.linkedId, response.startFocusedGenerationDateTimeUtc, response.endFocusedGenerationDateTimeUtc);
      })();
    }

    return () => { cancelled = true; };
  }, [backgroundQueries, activeModule?.mainQueryId]);

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
  if (activeModule?.mainQueryId) {
    setIsInputBlockedDueToServer(true);
  }
}, [activeModule?.mainQueryId]);

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

    triggerBackgroundQueriesPoll();

    setSendMessageQueryStatus("Completed");
    setLocalInput(""); // clear immediately
    localStorage.setItem(`chatInput_${activeModule.chatId}`, "");
    setActiveModule((prev) => prev ? { ...prev, currentUserInputValue: "", lastPlayerMessageId: "" } : prev);
    
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
          thinkingContent: "",
          createdAtUtc: null,
          sourceType: 1,
          messageIndex: (activeModule.nbColdMessages ?? 0) + messages.length + 2,
          summarized: false,
          characterAvatars: [],
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
          thinkingContent: "",
          createdAtUtc: null,
          sourceType: 1,
          messageIndex: (activeModule.nbColdMessages ?? 0) + messages.length + 2,
          summarized: false,
          characterAvatars: [],
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
      currentUserInputValue: "",
      latestPlayerDescription: ""
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
            networkError || backgroundQueriesNetworkError  || backgroundQueriesLoadingInitial ? undefined : 
            isInputBlockedDueToServer
              ? handleCancelLatestPlayerMessage
              : handleSendPlayerMessage
          }>
            {networkError || backgroundQueriesNetworkError ? (
              <LuServerOff />
            ) : (
              isInputBlockedDueToServer || backgroundQueriesLoadingInitial ? (
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
