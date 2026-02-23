import styles from "./ChatMessageComponent.module.css";
import { HiAdjustmentsHorizontal, HiBeaker, HiMiniUsers, HiChatBubbleLeftEllipsis, HiCircleStack, HiCog6Tooth, HiIdentification } from "react-icons/hi2";

export default function ChatMessageComponent() {
  return (
    <main className={styles.chatMessageComponent}>
      <div className={styles.container}>
        <div className={styles.leftMessageContainer}>
          <div className={styles.messageAvatarContainer}>
            <img src="./public/dev/Seyrdis.png" alt="Avatar" />
          </div>
          <div className={styles.messageInfoContainer}>
            <div title="messageId">#32</div>
            <div title="message generation time">75s</div>
            <div title="tokens in message">1305t</div>
          </div>
        </div>
        <div className={styles.messageContent}>
          <div className={styles.messageHeaderContent}>
            <div className={styles.messageHeaderContentName}>
              Azariel
            </div>
            <div className={styles.messageHeaderContentModel}>
              glm-reasoner (1m29s)
            </div>
            <div className={styles.messageHeaderContentCreatedAt}>
              February 22, 2026 7:45 PM
            </div>
          </div>
          <div className={styles.messageContentSeparator} />
          <div className={styles.messageContentValue}>
            hello 
          </div>
          <div className={styles.messageContentFooter}>
            <HiChatBubbleLeftEllipsis />
            <HiMiniUsers />
            <HiIdentification />
            <HiBeaker />
            <HiCircleStack />
            <HiAdjustmentsHorizontal />
            <HiCog6Tooth />
          </div>
        </div>
      </div>
    </main>
  );
}