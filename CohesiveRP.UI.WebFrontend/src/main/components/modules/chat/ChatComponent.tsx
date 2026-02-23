import styles from "./ChatComponent.module.css";
import ChatMessageComponent from "./message/ChatMessageComponent";
import UserInputComponent from "./userInput/UserInputComponent";

export default function ChatComponent() {
  return (
    <main className={styles.chatComponent}>
      <div className={styles.messagesContainer}>
        <ChatMessageComponent />
        <ChatMessageComponent />
        <ChatMessageComponent />
        <ChatMessageComponent />
        <ChatMessageComponent />
        <ChatMessageComponent />
      </div>
      <div className={styles.userInputContainer}>
        <UserInputComponent />
      </div>
    </main>
  );
}