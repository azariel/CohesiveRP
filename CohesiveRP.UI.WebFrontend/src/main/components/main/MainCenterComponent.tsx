import styles from "./MainCenterComponent.module.css";

/* modules components */
import ChatComponent from '../modules/chat/ChatComponent';

/* Store */
import { sharedContext } from '../../../store/AppSharedStoreContext';

export default function MainCenterComponent() {
  const { activeModule } = sharedContext();
  const renderModule = () => {
    console.log(`Rendering module [${activeModule}].`);
    switch (activeModule) {
      case "chat":
        return <ChatComponent />;
      default:
        return null;
    }
  };

  return (
    <main className={styles.body}>
      <div className={styles.centerModule}>
        {renderModule()}
      </div>
    </main>
  );
}