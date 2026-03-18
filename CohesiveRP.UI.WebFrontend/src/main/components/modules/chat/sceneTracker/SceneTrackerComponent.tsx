import { useRef, useEffect, useState } from "react";
import styles from "./SceneTrackerComponent.module.css";
import type { SharedContextChatType } from "../../../../../store/SharedContextChatType";
import { sharedContext } from "../../../../../store/AppSharedStoreContext";
import { getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { SceneTrackerResponseDto } from "../../../../../ResponsesDto/chat/SceneTrackerResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import { ImSpinner2 } from "react-icons/im";

export default function SceneTrackerComponent() {
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const didComponentMountAlready = useRef(false);
  const [sceneTrackerResponseDto, setSceneTrackerResponseDto] = useState<SceneTrackerResponseDto | null>(null);
  
  const fetchSceneTracker = async () => {
    try {
      if (!activeModule?.chatId)
        return;

      const response: SceneTrackerResponseDto | null = await getFromServerApiAsync<SceneTrackerResponseDto>(`api/sceneTrackers/${activeModule.chatId}`);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code != 200 || serverApiException?.message) {
        console.error(`Call to fetch specific chat scene tracker failed. [${JSON.stringify(serverApiException)}]`);
        return;
      }

      setSceneTrackerResponseDto(response);
    } catch (error) {
      console.error("Fetch scene tracker error:", error);
    }
  };

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;

    didComponentMountAlready.current = true;
    fetchSceneTracker();
  }, []);

  useEffect(() => {
    if (!activeModule?.sceneTrackerRefreshToken)
      return;

    fetchSceneTracker();
  }, [activeModule?.sceneTrackerRefreshToken]);

  const handleSaveSceneTracker = async (newContent: string) => {
    if (!activeModule?.chatId)
      return;

    // Optimistic update
    setSceneTrackerResponseDto((prev) =>
      prev ? ({ ...prev, sceneTracker: { ...prev.sceneTracker, content: newContent } } as SceneTrackerResponseDto) : prev
    );

    const response = await putToServerApiAsync(`api/sceneTrackers/${activeModule.chatId}`, {
      chatId: activeModule.chatId,
      content: newContent,
    });

    const serverApiException = response as ServerApiExceptionResponseDto | null;
    if (!response || serverApiException?.message) {
      console.error(`Updating scene tracker failed. [${JSON.stringify(serverApiException)}]`);
      fetchSceneTracker();
    }
  };

  const handleForceRefreshSceneTracker = async () => {
    if (!activeModule?.chatId)
      return;

    // TODO: activeModule?.sceneTrackerRefreshing true

    const response = await postToServerApiAsync(`api/sceneTrackers/${activeModule.chatId}`, null);

    // TODO: polling to refresh sceneTracker state

    const serverApiException = response as ServerApiExceptionResponseDto | null;
    if (!response || serverApiException?.message) {
      console.error(`Force refreshing scene tracker failed. [${JSON.stringify(serverApiException)}]`);
      fetchSceneTracker();
    }
  };

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
            {activeModule?.sceneTrackerRefreshing ? (
              <ImSpinner2 className={styles.spinnerLoading} />
            ) : (
              <div className={styles.contentAreaContainer}>
                <textarea
                  className={styles.contentArea}
                  value={sceneTrackerResponseDto?.sceneTracker?.content ?? "No scene tracker available."}
                  onChange={(e) =>
                    setSceneTrackerResponseDto((prev) =>
                      prev ? ({ ...prev, sceneTracker: { ...prev.sceneTracker, content: e.target.value } } as SceneTrackerResponseDto) : prev
                    )
                  }
                />
                <div className={styles.contentAreaButtons}>
                  <button
                    className={styles.contentAreaSaveBtn}
                    onClick={() => handleSaveSceneTracker(sceneTrackerResponseDto?.sceneTracker?.content ?? "")}
                  >
                    Save
                  </button>
                  <button
                    className={styles.contentAreaRefreshBtn}
                    onClick={handleForceRefreshSceneTracker}
                  >
                    Refresh
                  </button>
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </main>
  );
}