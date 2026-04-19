import styles from "./MainRightComponent.module.css";
import { sharedContext } from '../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../store/SharedContextChatType";
import { useChatMessages } from "../../../store/MessagesStoreContext";
import { getAvatarPathFromCharacterAvatarDefinition, GetAvatarPathFromChatIdAndAvatarId } from "../../../utils/avatarUtils";

export default function MainRightComponent() {
  const { activeModule } = sharedContext<SharedContextChatType>();
  const [messages] = useChatMessages(activeModule?.chatId);

  const lastAiMessage = [...messages]
    .reverse()
    .find((m) => m.sourceType === 1 && m.characterAvatars && m.characterAvatars.length > 0);

  const paths = lastAiMessage?.characterAvatars ?? [];
  const firstPath = paths[0];

  const avatarSrc = firstPath
    ? firstPath?.name !== null
      ? getAvatarPathFromCharacterAvatarDefinition(firstPath)
      : GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar")
    : null;

  // Paths [1–3] become the small avatars row (discard anything beyond index 3)
  const smallAvatarSrcs = paths
    .slice(1, 4)
    .map((p) =>
      p?.name !== null
        ? getAvatarPathFromCharacterAvatarDefinition(p)
        : GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar")
    );

  return (
    <main className={styles.body}>
      <div className={styles.centerModule}>
        {avatarSrc && (
          <>
            <div className={styles.avatarContainer}>
              <img src={avatarSrc} alt="Character avatar" className={styles.avatar} onError={(e) => { e.currentTarget.src = GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar"); }} />
            </div>

            {smallAvatarSrcs.length > 0 && (
              <div
                className={styles.smallAvatarsRow}
                style={{ gridTemplateColumns: `repeat(${Math.max(smallAvatarSrcs.length, 2)}, 1fr)` }}
              >
                {smallAvatarSrcs.map((src, i) => (
                  <div key={i} className={styles.smallAvatarContainer}>
                    <img src={src} alt={`Character avatar ${i + 2}`} onError={(e) => { e.currentTarget.src = GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar"); }} />
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