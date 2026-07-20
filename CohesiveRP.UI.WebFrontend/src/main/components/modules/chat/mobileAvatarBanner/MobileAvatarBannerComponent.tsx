import styles from "./MobileAvatarBannerComponent.module.css";
import { sharedContext } from "../../../../../store/AppSharedStoreContext";
import type { SharedContextChatType } from "../../../../../store/SharedContextChatType";
import { useChatMessages } from "../../../../../store/MessagesStoreContext";
import {
  getAvatarPathFromCharacterAvatarDefinition,
  GetAvatarPathFromChatIdAndAvatarId,
  GetFallbackEmpty,
} from "../../../../../utils/avatarUtils";

/** Native dimensions of avatar images. Adjust if they ever change. */
const AVATAR_W = 832;
const AVATAR_H = 1216;

export default function MobileAvatarBannerComponent() {
  
  const { activeModule } = sharedContext<SharedContextChatType>();

  if (activeModule?.hideAvatars === true) {
    return null;
  }

  const [messages] = useChatMessages(activeModule?.chatId);
  
  const lastAiMessage = [...messages]
    .reverse()
    .find(
      (m) =>
        m.sourceType === 1 &&
        m.characterAvatars &&
        m.characterAvatars.length > 0
    );

  const avatars = (lastAiMessage?.characterAvatars ?? []).slice(0, 4);

  if (avatars.length === 0) return null;

  const chatId = activeModule?.chatId ?? "";
  const count = avatars.length;

  const makeFallback =
    (chatId: string) =>
    (e: React.SyntheticEvent<HTMLImageElement>) => {
      const el = e.currentTarget;
      el.onerror = () => {
        el.onerror = null;
        el.src = GetFallbackEmpty();
      };
      el.src = GetAvatarPathFromChatIdAndAvatarId(chatId, "avatar");
    };

  return (
    <div className={styles.banner}>
      <span className={styles.accentLine} />

      <div
        className={styles.strip}
        style={{
          width: `${count * 25}%`,
          gridTemplateColumns: `repeat(${count}, 1fr)`,
          aspectRatio: `${count * AVATAR_W} / ${AVATAR_H}`,
        }}
      >
        {avatars.map((avatar, i) => {
          const src =
            avatar.name !== null
              ? getAvatarPathFromCharacterAvatarDefinition(avatar)
              : GetAvatarPathFromChatIdAndAvatarId(chatId, "avatar");

          return (
            <div key={i} className={styles.avatarCell}>
              <img
                src={src}
                alt={avatar.name ?? `Character avatar ${i + 1}`}
                className={styles.avatarImg}
                onError={makeFallback(chatId)}
              />
              <div className={styles.cellFade} />
              {avatar.expression && (
                <span className={styles.expressionLabel}>
                  {avatar.expression}
                </span>
              )}
            </div>
          );
        })}
      </div>

      <span className={styles.accentLineBottom} />
    </div>
  );
}