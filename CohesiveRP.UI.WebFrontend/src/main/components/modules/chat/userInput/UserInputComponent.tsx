import styles from "./UserInputComponent.module.css";
import { useRef, useState, useEffect  } from "react";
import { HiChip } from "react-icons/hi";
import { BiSolidPaperPlane, BiPaperPlane  } from "react-icons/bi";
import { ImSpinner2 } from "react-icons/im";

// Backend webapi
import { postToServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";

// Store
import { sharedContext } from '../../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../../store/SharedContextChatType";
import type { ChatMessageResponseDto } from "../../../../../ResponsesDto/chat/BusinessObjects/ChatMessageResponseDto";

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
    setActiveModule({...activeModule, messages: [...(activeModule.messages || []), response.messageObj]});
  };

  const handleCancelLatestPlayerMessage = () => {
    // optional: cancel request / noop / show tooltip
    console.log("Cancelling...");
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
