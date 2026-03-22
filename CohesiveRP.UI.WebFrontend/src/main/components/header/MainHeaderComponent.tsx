import styles from "./MainHeaderComponent.module.css";
import { LuBlocks } from "react-icons/lu";
import { HiAdjustmentsHorizontal, HiBeaker, HiMiniUsers, HiChatBubbleLeftEllipsis, HiCircleStack, HiCog6Tooth, HiIdentification } from "react-icons/hi2";

/* Store */
import { sharedContext } from '../../../store/AppSharedStoreContext';
import type { SharedContextType } from "../../../store/SharedContextType";
import { FaBook } from "react-icons/fa";

export default function Header() {
  const { navigateTo } = sharedContext();

  const handleIconClick = (moduleName: string) => {

    let module = {
      moduleName: moduleName
    } as SharedContextType;

    navigateTo(module);
    console.log(`Module [${moduleName}] selected.`);
  };

  return (
    <header className={styles.header}>
      <div className={styles.iconRow}>
        <div className={styles.iconRowLeft}>
          <button className={styles.iconBtn} onClick={() => handleIconClick("chatSelection")} aria-label="Chat Selection Module" title="Chat">
            <HiChatBubbleLeftEllipsis />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("characters")} aria-label="Characters Module"  title="Characters">
            <HiMiniUsers />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("personas")} aria-label="Player Module"  title="Player">
            <HiIdentification />
          </button>
          <button className={styles.iconBtnSmaller} onClick={() => handleIconClick("lorebooks")} aria-label="LoreBooks Module"  title="Lorebooks">
            <FaBook />
          </button>
        </div>
        <div className={styles.iconRowRight}>
          {/* <button className={styles.iconBtn} onClick={() => handleIconClick("experimentation")} aria-label="Experimentation Module" title="Experimentation">
            <HiBeaker />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("agents")} aria-label="Agents Module" title="Agents">
            <HiChip />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("storage")} aria-label="Storage Module" title="Storage">
            <HiCircleStack />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("adjustments")} aria-label="Adjustments Module" title="Adjustments">
            <HiAdjustmentsHorizontal />
          </button> */}
          <button className={styles.iconBtnCompletionPresets} onClick={() => handleIconClick("chatCompletionPresets")} aria-label="CompletionPresets" title="Completion Presets">
            <LuBlocks  />
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