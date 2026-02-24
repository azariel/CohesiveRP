import styles from "./ChatSelectionComponent.module.css";
import { FaFilter  } from "react-icons/fa";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';

export default function ChatSelectionComponent() {
  const { setActiveModule } = sharedContext();
  
    const handleSpecificChatClick = (moduleName: string) => {
      setActiveModule(moduleName);
      console.log(`Chat selected -> Module [${moduleName}] selected.`);
    };

  return (
    <main className={styles.chatSelectionComponent}>
      <div className={styles.filterRow}>
        <FaFilter />
      </div>
      <div className={styles.chatsGridContainer}>
        {Array.from({ length: 16 }).map((_, index) => (
          <div>
            <label className={styles.chatCharNameLabel}>char name {index}</label>
            <div
              key={index}
              className={styles.chatAvatarContainer}
              onClick={() => handleSpecificChatClick("chat")}
            >
              <img src="./public/dev/Seyrdis.png" alt="Avatar" />
            </div>
            <label className={styles.chatFootLabel}>2026-02-24 14h22m24s</label>
          </div>
        ))}
      </div>
    </main>
  );
}