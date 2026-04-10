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
    .find((m) => m.sourceType === 1 && m.avatarsFilePath && m.avatarsFilePath.length > 0);

  const paths = lastAiMessage?.avatarsFilePath ?? [];
  const firstPath = paths[0];

  const avatarSrc = firstPath
    ? firstPath !== "avatar"
      ? GetAvatarPathFromAvatarFilePath(firstPath)
      : GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar")
    : null;

  // Paths [1–3] become the small avatars row (discard anything beyond index 3)
  const smallAvatarSrcs = paths
    .slice(1, 4)
    .map((p) =>
      p !== "avatar"
        ? GetAvatarPathFromAvatarFilePath(p)
        : GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar")
    );

  return (
    <main className={styles.body}>
      <div className={styles.centerModule}>
        {avatarSrc && (
          <>
            <div className={styles.avatarContainer}>
              <img src={avatarSrc} alt="Character avatar" className={styles.avatar} />
            </div>

            {smallAvatarSrcs.length > 0 && (
              <div
                className={styles.smallAvatarsRow}
                style={{ gridTemplateColumns: `repeat(${smallAvatarSrcs.length}, 1fr)` }}
              >
                {smallAvatarSrcs.map((src, i) => (
                  <div key={i} className={styles.smallAvatarContainer}>
                    <img src={src} alt={`Character avatar ${i + 2}`} className={styles.smallAvatar} />
                  </div>
                ))}
              </div>
            )}
          </>
        )}
      </div>
    </main>
  );
}