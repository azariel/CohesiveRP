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
const MAX_HEIGHT_PX = 280;

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

  const allAvatarSrcs = paths.slice(0, 4).map((p) =>
    p?.name !== null
      ? getAvatarPathFromCharacterAvatarDefinition(p)
      : GetAvatarPathFromChatIdAndAvatarId(activeModule?.chatId ?? "", "avatar")
  );

  if (allAvatarSrcs.length === 0) return null;

  const chatId = activeModule?.chatId ?? "";
  const count = allAvatarSrcs.length;

  // Height that gives each cell exactly the native portrait ratio:
  //   cellWidth  = 100vw / count
  //   cellHeight = cellWidth × (AVATAR_H / AVATAR_W)
  //              = (100 / count) × (AVATAR_H / AVATAR_W)  vw
  // For N=2 on 390px: (100/2) × (1216/832) ≈ 73vw ≈ 285px  → fits, close to perfect ratio
  // For N=3 on 390px: (100/3) × (1216/832) ≈ 49vw ≈ 190px  → perfect ratio
  // For N=4 on 390px: (100/4) × (1216/832) ≈ 37vw ≈ 142px  → perfect ratio
  // For N=1 on 390px: (100/1) × (1216/832) ≈ 146vw ≈ 570px → capped at MAX_HEIGHT_PX
  const heightVw = (AVATAR_H / AVATAR_W / count) * 100;
  const adjustedHeightVw = count === 1 ? heightVw / 2 : heightVw;

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

    <div className={count === 1 ? styles.stripSingleWrapper : undefined}>
      <div
        className={styles.strip}
        style={{
          height: `min(${adjustedHeightVw.toFixed(2)}vw, ${MAX_HEIGHT_PX}px)`,
          gridTemplateColumns: `repeat(${count}, 1fr)`,
        }}
      >
        {allAvatarSrcs.map((src, i) => (
          <div key={i} className={styles.avatarCell}>
            <img
              src={src}
              alt={`Character avatar ${i + 1}`}
              className={styles.avatarImg}
              onError={makeFallback(chatId)}
            />
            <div className={styles.cellFade} />
          </div>
        ))}
      </div>
    </div>

    <span className={styles.accentLineBottom} />
  </div>
);
}
