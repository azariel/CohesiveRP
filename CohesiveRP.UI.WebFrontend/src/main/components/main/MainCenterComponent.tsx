import styles from "./MainCenterComponent.module.css";

/* modules components */
import ChatComponent from '../modules/chat/ChatComponent';
import ChatSelectionComponent from "../modules/chatsSelection/ChatSelectionComponent";
import CharactersComponent from "../modules/charactersSelection/CharactersComponent";
import SettingsComponent from "../modules/settings/SettingsComponent";

/* Store */
import { sharedContext } from '../../../store/AppSharedStoreContext';

export default function MainCenterComponent() {
  const { activeModule } = sharedContext();
  const renderModule = () => {
    console.log(`Rendering module [${activeModule?.moduleName}].`);
    switch (activeModule.moduleName) {
      case "chatSelection":
        return <ChatSelectionComponent />;
      case "chat":
        return <ChatComponent />;
      case "characters":
        return <CharactersComponent />;
        case "settings":
        return <SettingsComponent />;
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