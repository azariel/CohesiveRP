import styles from "./MainLeftComponent.module.css";
import { sharedContext } from '../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../store/SharedContextChatType";
import { useChatMessages } from "../../../store/MessagesStoreContext";
import { GetAvatarPathFromPersonaAvatarDefinition, GetAvatarPathFromChatIdAndAvatarId } from "../../../utils/avatarUtils";

export default function MainLeftComponent() {
  const { activeModule } = sharedContext<SharedContextChatType>();
  const [messages] = useChatMessages(activeModule?.chatId);

  const lastPersonaMessage = [...messages]
    .reverse()
    .find((m) => m.sourceType === 0 && m.characterAvatars && m.characterAvatars.length > 0);

  const firstPath = lastPersonaMessage?.characterAvatars?.[0];
  const avatarSrc = firstPath
    ? firstPath?.name !== null
      ? GetAvatarPathFromPersonaAvatarDefinition(firstPath)
      : GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar")
    : null;

  return (
    <main className={styles.body}>
      <div className={styles.centerModule}>
        {avatarSrc && (
          <div className={styles.avatarContainer}>
            <img src={avatarSrc} alt="Persona avatar" className={styles.avatar} onError={(e) => { e.currentTarget.src = GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar"); }} />
          </div>
        )}
      </div>
    </main>
  );
}