import styles from "./MainHeaderComponent.module.css";
import { LuBlocks } from "react-icons/lu";
import { HiMiniUsers, HiChatBubbleLeftEllipsis, HiCog6Tooth, HiIdentification, HiRectangleStack } from "react-icons/hi2";
import { MdArrowBack } from "react-icons/md";
import { FaBook } from "react-icons/fa";

/* Store */
import { sharedContext } from '../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../store/SharedContextChatType";

export default function Header() {
  const { activeModule, setActiveModule, navigateTo } = sharedContext<SharedContextChatType>();

  const handleIconClick = (moduleName: string) => {
    const module = { moduleName } as unknown as SharedContextChatType;
    navigateTo(module);
    console.log(`Module [${moduleName}] selected.`);
  };

  const isInChat = activeModule?.moduleName === "chat" && !!activeModule?.chatId;
  const isViewingCharacterSheets = isInChat && activeModule?.chatSubView === "characterSheetInstances";

  const handleToggleCharacterSheets = () => {
    if (!isInChat) return;
    setActiveModule((prev) =>
      prev ? { ...prev, chatSubView: isViewingCharacterSheets ? "chat" : "characterSheetInstances" } : prev
    );
  };

  return (
    <header className={styles.header}>
      <div className={styles.iconRow}>
        <div className={styles.iconRowLeft}>
          <button className={styles.iconBtn} onClick={() => handleIconClick("chatSelection")} aria-label="Chat Selection Module" title="Chat">
            <HiChatBubbleLeftEllipsis />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("characters")} aria-label="Characters Module" title="Characters">
            <HiMiniUsers />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("personas")} aria-label="Player Module" title="Player">
            <HiIdentification />
          </button>
          <button className={styles.iconBtnSmaller} onClick={() => handleIconClick("lorebooks")} aria-label="LoreBooks Module" title="Lorebooks">
            <FaBook />
          </button>
        </div>
        <div className={styles.iconRowRight}>
          {isInChat && (
            <button
              className={`${styles.iconBtn} ${isViewingCharacterSheets ? styles.iconBtnActive : ""}`}
              onClick={handleToggleCharacterSheets}
              aria-label={isViewingCharacterSheets ? "Back to Chat" : "Character Sheets"}
              title={isViewingCharacterSheets ? "Back to Chat" : "Character Sheets"}
            >
              {isViewingCharacterSheets ? <MdArrowBack /> : <HiRectangleStack />}
            </button>
          )}
          <button className={styles.iconBtnCompletionPresets} onClick={() => handleIconClick("chatCompletionPresets")} aria-label="CompletionPresets" title="Completion Presets">
            <LuBlocks />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("settings")} aria-label="Settings Module" title="Settings">
            <HiCog6Tooth />
          </button>
        </div>
      </div>
      <div className={styles.separator} />
    </header>
  );
}