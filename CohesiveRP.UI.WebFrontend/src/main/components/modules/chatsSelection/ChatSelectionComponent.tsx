import styles from "./ChatSelectionComponent.module.css";
import { useEffect, useState, useRef  } from "react";
import { FaFilter  } from "react-icons/fa";
import { ImSpinner2 } from "react-icons/im";
import { AiOutlineDisconnect } from "react-icons/ai";
import { MdAddBox, MdEdit, MdDelete } from "react-icons/md";

// Backend webapi
import { getFromServerApiAsync, deleteFromServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { SelectableChatsResponseDto } from "../../../../ResponsesDto/chatSelection/SelectableChatsResponseDto";
import type { SelectableChatResponseDto } from "../../../../ResponsesDto/chatSelection/SelectableChatResponseDto";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../store/SharedContextChatType";
import { GetChatNameFontSize } from "../../../../utils/fontSizeUtils";
import type { SharedContextType } from "../../../../store/SharedContextType";
import { GetAvatarPathFromChatId } from "../../../../utils/avatarUtils";

export default function ChatSelectionComponent() {
  const { navigateTo } = sharedContext();
  const [isLoading, setIsLoading] = useState(true);
  const didComponentMountAlready = useRef(false);
  const [chatDefinitions, setChatDefinitions] = useState<SelectableChatsResponseDto>();
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [hoveredChatId, setHoveredChatId] = useState<string | null>(null);
  const [deletingChatId, setDeletingChatId] = useState<string | null>(null);

  useEffect(() => {
    if (didComponentMountAlready.current)
      return;

    didComponentMountAlready.current = true;

    const fetchData = async () => {
      setIsLoading(true);
      try {
        const response: SelectableChatsResponseDto | null = await getFromServerApiAsync<SelectableChatsResponseDto>("api/chats");

        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch all chats failed. [${serverApiException}] [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setChatDefinitions({ code: -1, chats: [] });
          return;
        }

        setChatDefinitions(response ?? []);
        console.log(`All chats fetched successfully.`);
      } catch (error) {
        console.error("Fetch error:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleCreateNewChatClick = async () => {
    navigateTo({ moduleName: 'characters' } as SharedContextType);
  };

  const handleSpecificChatClick = async (chat: SelectableChatResponseDto) => {
    setIsLoading(true);
    try {
      const savedInput = localStorage.getItem(`chatInput_${chat.chatId}`) ?? "";
      navigateTo({
        chatId: chat.chatId,
        moduleName: "chat",
        currentUserInputValue: savedInput,
      } as SharedContextChatType);
    } finally {
      setIsLoading(false);
    }
  };

  const handleEditChatClick = (e: React.MouseEvent, chat: SelectableChatResponseDto) => {
    e.stopPropagation();
    navigateTo({
      chatId: chat.chatId,
      moduleName: "chatDetails",
    } as SharedContextType);
  };

  const handleDeleteChatClick = async (e: React.MouseEvent, chat: SelectableChatResponseDto) => {
    e.stopPropagation();
    setDeletingChatId(chat.chatId);
    try {
      const response = await deleteFromServerApiAsync(`api/chats/${chat.chatId}`);
      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Failed to delete chat [${chat.chatId}].`);
        return;
      }

      setChatDefinitions((prev) =>
        prev
          ? { ...prev, chats: prev.chats.filter((c) => c.chatId !== chat.chatId) }
          : prev
      );
      console.log(`Chat [${chat.chatId}] deleted successfully.`);
    } catch (error) {
      console.error("Delete error:", error);
    } finally {
      setDeletingChatId(null);
    }
  };

  return (
    <main className={styles.chatSelectionComponent}>
      <div className={styles.filterRow}>
        <FaFilter />
      </div>
      <div className={styles.chatsGridContainer}>
        {isLoading ? (
          <div className={styles.chatAddNewChatContainer}>
            <ImSpinner2 className={styles.isLoadingSpinner} />
          </div>
        ) : isNetworkDown ? (
          <div className={styles.chatAddNewChatContainerDisconnected}>
            <AiOutlineDisconnect className={styles.chatAddNewChatBtn} />
          </div>
        ) : (
          <div className={styles.chatAddNewChatContainer} onClick={handleCreateNewChatClick}>
            <MdAddBox className={styles.chatAddNewChatBtn} />
          </div>
        )}

        {!isLoading && chatDefinitions?.chats && chatDefinitions.chats.length > 0 ? (
          chatDefinitions.chats.map((chat, index) => (
            <div
              key={index}
              className={styles.chatCard}
              onMouseEnter={() => setHoveredChatId(chat.chatId)}
              onMouseLeave={() => setHoveredChatId(null)}
            >
              <label
                className={styles.chatCharNameLabel}
                style={{ fontSize: GetChatNameFontSize(chat.name ?? "") }}
              >
                {chat.name}
              </label>

              <div className={styles.chatAvatarContainer}>
                <img src={GetAvatarPathFromChatId(chat.chatId)} alt="Avatar" />
              </div>

              {/* Hover overlay */}
                <div className={`${styles.chatCardOverlay} ${hoveredChatId === chat.chatId ? styles.chatCardOverlayVisible : ""}`}
                onClick={async () => await handleSpecificChatClick(chat)}>
                  <button
                    className={styles.chatCardActionBtn}
                    onClick={(e) => handleEditChatClick(e, chat)}
                    title="Edit chat"
                  >
                    <MdEdit className={styles.chatEditButton} />
                  </button>
                  <button
                    className={styles.chatCardActionBtn}
                    onClick={async (e) => await handleDeleteChatClick(e, chat)}
                    disabled={deletingChatId === chat.chatId}
                    title="Delete chat"
                  >
                    {deletingChatId === chat.chatId
                      ? <ImSpinner2 className={styles.isLoadingSpinner} />
                      : <MdDelete className={styles.chatCardDeleteBtn} />}
                  </button>
                </div>

              <label className={styles.chatFootLabel}>
                {chat.lastActivityDateTime?.toDateString() ?? ""}
              </label>
            </div>
          ))
        ) : (
          !isLoading && (
            isNetworkDown ? (
              <div className={styles.addChatTutorial}>
                <label>CohesiveRP backend is unreachable</label>
              </div>
            ) : (
              <div />
            )
          )
        )}
      </div>
    </main>
  );
}
