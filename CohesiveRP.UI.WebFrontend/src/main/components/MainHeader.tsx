import styles from "./MainHeader.module.css";
import { HiChip} from "react-icons/hi";
import { HiAdjustmentsHorizontal, HiBeaker, HiMiniUsers, HiChatBubbleLeftEllipsis, HiCircleStack, HiCog6Tooth, HiIdentification } from "react-icons/hi2";

export default function Header() {
  return (
    <header className={styles.header}>
      <div className={styles.iconRow}>
        <div className={styles.iconRowLeft}>
          <HiChatBubbleLeftEllipsis  className={styles.icon} />
          <HiMiniUsers className={styles.icon} />
          <HiIdentification className={styles.icon} />
          
        </div>
        <div className={styles.iconRowRight}>
          <HiBeaker className={styles.icon} />
          <HiChip className={styles.icon} />
          <HiCircleStack className={styles.icon} />
          <HiAdjustmentsHorizontal className={styles.icon} />
          <HiCog6Tooth  className={styles.icon} />
        </div>
      </div>
    <div className={styles.separator} />
    </header>
  );
}