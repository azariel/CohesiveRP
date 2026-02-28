import type { ChatMessage } from "../../../../../ResponsesDto/chat/BusinessObjects/ChatMessage";
import styles from "./ChatMessageComponent.module.css";
import { HiAdjustmentsHorizontal, HiBeaker, HiMiniUsers, HiChatBubbleLeftEllipsis, HiCircleStack, HiCog6Tooth, HiIdentification, HiMiniChevronRight } from "react-icons/hi2";

interface Props {
  messageContent?: ChatMessage;
  enableSwipeBtn?: boolean;
}

export default function ChatMessageComponent({ messageContent, enableSwipeBtn = false }: Props) {
  return (
    <main className={styles.chatMessageComponent}>
      <div className={styles.container}>
        <div className={styles.leftMessageContainer}>
          <div className={styles.messageAvatarContainer}>
            <img src="./dev/Seyrdis.png" alt="Avatar" />
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
            {messageContent?.content} 
          </div>
          <div className={styles.messageContentFooter}>
            <div className={styles.messageContentFooterLeftSideIcons}>
              <HiChatBubbleLeftEllipsis />
              <HiMiniUsers />
              <HiIdentification />
              <HiBeaker />
              <HiCircleStack />
              <HiAdjustmentsHorizontal />
              <HiCog6Tooth />
            </div>
            {enableSwipeBtn ? (
              <div className={styles.messageContentFooterRightSideSwipeIcons}>
                <label className={styles.messageContentFooterRightSideSwipeIconsLabel}>1/1</label>
                <HiMiniChevronRight className={styles.messageContentFooterRightSideSwipeIconsBtn} />
              </div>
            ) : (
              <div></div>// empty
            )}
          </div>
        </div>
      </div>
    </main>
  );
}