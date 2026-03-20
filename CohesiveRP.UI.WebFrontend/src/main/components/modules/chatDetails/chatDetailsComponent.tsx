import styles from "./ChatDetailsComponent.module.css";
import { useRef, useEffect, useState } from "react";
import { AiOutlineDisconnect } from "react-icons/ai";
import { ImSpinner2 } from "react-icons/im";
import { MdAddBox } from "react-icons/md";

import { deleteFromServerApiAsync, getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../store/SharedContextChatType";
import type { ChatResponseDto } from "../../../../ResponsesDto/chat/ChatResponseDto";
import type { SharedContextType } from "../../../../store/SharedContextType";
import { GetAvatarPathFromCharacterId, GetAvatarPathFromChatId } from "../../../../utils/avatarUtils";

export default function ChatDetailsComponent() {
  const { activeModule } = sharedContext<SharedContextChatType>();
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [isLoadingChatDetails, setIsLoadingChatDetails] = useState(true);
  const [chatResponse, setChatResponse] = useState<ChatResponseDto | null>(null);

  // saving state
  const [name, setName] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [operationError, setOperationError] = useState(false);

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;
    didComponentMountAlready.current = true;

    fetchChatDetails();
  }, []);

  const fetchChatDetails = async () => {

      if(!activeModule?.chatId){
        console.error(`Selected chat id is not valid and thus has no details.`);
        return;
      }

      try {
        setIsLoadingChatDetails(true);
        const response: ChatResponseDto | null = await getFromServerApiAsync<ChatResponseDto>(`api/chats/${activeModule.chatId}`);
        
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch chat details failed. [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setChatResponse({
            code : -1,
            id: "",
            lastActivityDateTime: null,
            name: null,
          });

          return;
        }
        
        console.log(`Chat details fetched successfully.`);
        setChatResponse(response);
        setName(response?.name ?? "");
      } catch (error) {
        console.error("Fetch chat error:", error);
      } finally {
        setIsLoadingChatDetails(false);
      }
    };

  const handleSave = async () => {
    if (!activeModule?.chatId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {
      const response = await putToServerApiAsync(`api/chats/${activeModule.chatId}`, { 
        name
      });

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Save failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      }

    } catch (error) {
      console.error("Save chat details error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!activeModule?.chatId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {

      const response = await deleteFromServerApiAsync(`api/chats/${activeModule.chatId}`);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Deletion failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      } else{
        let module = {
          moduleName: "chats"
        } as SharedContextType;

        navigateTo(module);
      }

    } catch (error) {
      console.error("Deletion chat error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <main className={styles.chatDetailsComponent}>
      {isNetworkDown ? (
          <div className={styles.networkDownContainer}>
            <AiOutlineDisconnect className={styles.networkDownIcon} />
            <label>CohesiveRP backend is unreachable</label>
          </div>
        ) : (
        isLoadingChatDetails ? (
          <ImSpinner2 className={ styles.loadingChatDetailsSpinner } />
        ):(
          <div className={styles.chatDetailsContainer}>
            <div className={styles.chatHeaderContainer}>
              <div className={styles.chatAvatarContainer}>
                <div className={styles.addNewAvatarImageContainer}>
                  <input
                      type="file"
                      style={{ display: "none" }}
                  />
                  <img src={`${GetAvatarPathFromChatId(chatResponse?.id ?? "")}`} alt="no image" />
                </div>
              </div>
            </div>
            <div className={styles.detailsContainer}>
              <div className={styles.chatNameContainer}>
                <label className={styles.chatNameLabel}>Chat Name</label>
                <textarea
                  className={styles.chatName}
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />
                <label className={styles.chatId}>{chatResponse?.id ?? ""}</label>
              </div>
              <div className={styles.operationsButtonsContainer}>
                <div className={styles.operationsButtons}>
                  <button className={styles.deleteButton} onClick={handleDelete} disabled={isSaving}>
                  {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Delete"}
                  </button>
                  <button className={styles.saveButton} onClick={handleSave} disabled={isSaving}>
                    {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Save"}
                  </button>
                </div>
                {operationError && (
                  <label className={styles.saveErrorLabel}>Failed to save/delete. Please try again.</label>
                )}
              </div>
            </div>
          </div>
        )
      )}
    </main>
  );
}