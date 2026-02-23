import styles from "./MainHeaderComponent.module.css";
import { HiChip } from "react-icons/hi";
import { HiAdjustmentsHorizontal, HiBeaker, HiMiniUsers, HiChatBubbleLeftEllipsis, HiCircleStack, HiCog6Tooth, HiIdentification } from "react-icons/hi2";

/* Store */
import { sharedContext } from '../../../store/AppSharedStoreContext';

export default function Header() {
  const { setActiveModule } = sharedContext();

  const handleIconClick = (moduleName: string) => {
    setActiveModule(moduleName);
    console.log(`Module [${moduleName}] selected.`);
  };

  return (
    <header className={styles.header}>
      <div className={styles.iconRow}>
        <div className={styles.iconRowLeft}>
          <button className={styles.iconBtn} onClick={() => handleIconClick("chat")} aria-label="Chat Module" title="Chat">
            <HiChatBubbleLeftEllipsis />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("characters")} aria-label="Characters Module"  title="Characters">
            <HiMiniUsers />
          </button>
          <button className={styles.iconBtn} onClick={() => handleIconClick("player")} aria-label="Player Module"  title="Player">
            <HiIdentification />
          </button>
        </div>
        <div className={styles.iconRowRight}>
          <button className={styles.iconBtn} onClick={() => handleIconClick("experimentation")} aria-label="Experimentation Module" title="Experimentation">
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