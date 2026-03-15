import { useRef, useEffect, useState } from "react";
import styles from "./SceneTrackerComponent.module.css";
import type { SharedContextChatType } from "../../../../../store/SharedContextChatType";
import { sharedContext } from "../../../../../store/AppSharedStoreContext";
import { getFromServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { SceneTrackerResponseDto } from "../../../../../ResponsesDto/chat/SceneTrackerResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";

export default function SceneTrackerComponent() {
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const didComponentMountAlready = useRef(false);
  const [sceneTrackerResponseDto, setSceneTrackerResponseDto] = useState<SceneTrackerResponseDto | null>(null);

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;

    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        if(!activeModule?.chatId) {
          console.error(`Couldn't load chat scene tracker. ChatId was undefined.`);
          return;
        }

        const response: SceneTrackerResponseDto | null = await getFromServerApiAsync<SceneTrackerResponseDto>(`api/sceneTrackers/${activeModule.chatId}`);
        
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch specific chat scene tracker failed. [${JSON.stringify(serverApiException)}]`);
          return;
        }

        setSceneTrackerResponseDto(response);
        console.log(`Specific scene tracker for chat [${activeModule.chatId}] fetched successfully.`);
      } catch (error) {
        console.error("Fetch scene tracker error:", error);
      }
    };

    fetchData();
    
  }, []);

  return (
    <main className={styles.chatMessageComponent}>
      <div className={styles.collapsible}>
        <button
          className={styles.collapseToggle}
          onClick={() => setActiveModule((prev) => prev ? { ...prev, isSceneTrackerOpened: !prev.isSceneTrackerOpened } : prev)}
          aria-expanded={activeModule?.isSceneTrackerOpened ?? false}
        >
          <span className={styles.toggleLabel}>Scene Tracker</span>
          <span className={`${styles.toggleIcon} ${activeModule?.isSceneTrackerOpened ? styles.open : ""}`}>▾</span>
        </button>

        {activeModule?.isSceneTrackerOpened && (
          <div className={styles.collapseBody}>
            <textarea
              className={styles.contentArea}
              readOnly
              value={sceneTrackerResponseDto?.sceneTracker?.content ?? "No scene tracker available."}
            />
          </div>
        )}
      </div>
    </main>
  );
}