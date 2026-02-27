import styles from "./ChatComponent.module.css";
import { useEffect, useRef } from "react";
import ChatMessageComponent from "./message/ChatMessageComponent";
import UserInputComponent from "./userInput/UserInputComponent";

export default function ChatComponent() {
  const messagesRef = useRef<HTMLDivElement>(null);

  // Scroll to bottom when component mounts
  useEffect(() => {
    if (messagesRef.current) {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
  }, []);

  return (
    <main className={styles.chatComponent}>
      <div className={styles.messagesContainer} ref={messagesRef}>
        <ChatMessageComponent />
        <ChatMessageComponent enableSwipeBtn={true} />
        <div className={styles.userInputContainer}>
          <UserInputComponent messagesRef={messagesRef} />
        </div>
      </div>
    </main>
  );
}