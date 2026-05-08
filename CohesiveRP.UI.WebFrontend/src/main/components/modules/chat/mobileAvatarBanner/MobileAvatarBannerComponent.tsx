import styles from "./MobileAvatarBannerComponent.module.css";
import { sharedContext } from "../../../../../store/AppSharedStoreContext";
import type { SharedContextChatType } from "../../../../../store/SharedContextChatType";
import { useChatMessages } from "../../../../../store/MessagesStoreContext";
import {
  getAvatarPathFromCharacterAvatarDefinition,
  GetAvatarPathFromChatIdAndAvatarId,
  GetFallbackEmpty,
} from "../../../../../utils/avatarUtils";

export default function MobileAvatarBannerComponent() {
  const { activeModule } = sharedContext<SharedContextChatType>();
  const [messages] = useChatMessages(activeModule?.chatId);

  const lastAiMessage = [...messages]
    .reverse()
    .find(
      (m) =>
        m.sourceType === 1 &&
        m.characterAvatars &&
        m.characterAvatars.length > 0
    );

  const paths = lastAiMessage?.characterAvatars ?? [];
  const firstPath = paths[0];

  const avatarSrc = firstPath
    ? firstPath?.name !== null
      ? getAvatarPathFromCharacterAvatarDefinition(firstPath)
      : GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar")
    : null;

  const smallAvatarSrcs = paths
    .slice(1, 4)
    .map((p) =>
      p?.name !== null
        ? getAvatarPathFromCharacterAvatarDefinition(p)
        : GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar")
    );

  if (!avatarSrc) return null;

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

  const chatId = activeModule?.chatId ?? "";

  return (
    <div className={styles.banner}>
      {/* Decorative cyan rule */}
      <span className={styles.accentLine} />

      <div className={styles.strip}>
        {/* ── Primary avatar ── */}
        <div className={styles.primaryFrame}>
          <img
            src={avatarSrc}
            alt="Character avatar"
            className={styles.primaryImg}
            onError={makeFallback(chatId)}
          />
          <div className={styles.primaryFade} />
        </div>

        {/* ── Secondary avatars ── */}
        {smallAvatarSrcs.length > 0 && (
          <div
            className={styles.secondaryGrid}
            style={{
              gridTemplateRows: `repeat(${smallAvatarSrcs.length}, 1fr)`,
            }}
          >
            {smallAvatarSrcs.map((src, i) => (
              <div key={i} className={styles.secondaryFrame}>
                <img
                  src={src}
                  alt={`Character avatar ${i + 2}`}
                  className={styles.secondaryImg}
                  onError={makeFallback(chatId)}
                />
                <div className={styles.secondaryFade} />
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Decorative bottom rule */}
      <span className={styles.accentLineBottom} />
    </div>
  );
}
