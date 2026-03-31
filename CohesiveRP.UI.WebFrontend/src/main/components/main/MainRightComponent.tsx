import styles from "./MainRightComponent.module.css";
import { sharedContext } from '../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../store/SharedContextChatType";
import { useChatMessages } from "../../../store/MessagesStoreContext";
import { GetAvatarPathFromAvatarFilePath, GetAvatarPathFromChatIdAndAvatarId } from "../../../utils/avatarUtils";

export default function MainRightComponent() {
  const { activeModule } = sharedContext<SharedContextChatType>();
  const [messages] = useChatMessages(activeModule?.chatId);

  const lastAiMessage = [...messages]
    .reverse()
    .find((m) => m.sourceType === 1  && m.avatarFilePath);

  const avatarSrc = lastAiMessage
    ? lastAiMessage.avatarFilePath && lastAiMessage.avatarFilePath !== "avatar"
      ? GetAvatarPathFromAvatarFilePath(lastAiMessage.avatarFilePath)
      : GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar")
    : null;

  return (
    <main className={styles.body}>
      <div className={styles.centerModule}>
        {avatarSrc && (
          <div className={styles.avatarContainer}>
            <img src={avatarSrc} alt="Character avatar" className={styles.avatar} />
          </div>
        )}
      </div>
    </main>
  );
}