import styles from "./MainCenterComponent.module.css";

/* modules components */
import ChatComponent from '../modules/chat/ChatComponent';
import ChatSelectionComponent from "../modules/chatsSelection/ChatSelectionComponent";
import CharactersComponent from "../modules/charactersSelection/CharactersSelectionComponent";
import CharacterDetailsComponent from "../modules/characterDetails/CharacterDetailsComponent";
import SettingsComponent from "../modules/settings/SettingsComponent";

/* Store */
import { sharedContext } from '../../../store/AppSharedStoreContext';
import PersonasSelectionComponent from "../modules/personasSelection/PersonasSelectionComponent";
import PersonaDetailsComponent from "../modules/personaDetails/PersonaDetailsComponent";
import LorebookSelectionComponent from "../modules/lorebooksSelection/LorebookSelectionComponent";
import LorebookDetailsComponent from "../modules/lorebookDetails/LorebookDetailsComponent";

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
      case "personas":
        return <PersonasSelectionComponent />;
      case "lorebooks":
        return <LorebookSelectionComponent />;
      case "lorebookDetails":
        return <LorebookDetailsComponent />;
      case "characterDetails":
        return <CharacterDetailsComponent />;
      case "personaDetails":
        return <PersonaDetailsComponent />;
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