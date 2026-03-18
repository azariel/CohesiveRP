import styles from "./LorebookDetailsComponent.module.css";
import { useRef, useEffect, useState, Fragment } from "react";
import { AiOutlineDisconnect } from "react-icons/ai";
import { ImSpinner2 } from "react-icons/im";
import { MdAddBox } from "react-icons/md";

import { deleteFromServerApiAsync, getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextLorebookType } from "../../../../store/SharedContextLorebookType";
import type { LorebookResponseDto } from "../../../../ResponsesDto/lorebooks/LorebookResponseDto";
import { GetAvatarPathFromLorebookId } from "../../../../utils/avatarUtils";
import type { SharedContextType } from "../../../../store/SharedContextType";
import type { LorebookUpdateRequestDto } from "../../../../RequestDto/lorebooks/LorebookUpdateRequestDto";
import LorebookEntryComponent from "./lorebookEntry/LorebookEntryComponent";
import type { LorebookEntry } from "../../../../ResponsesDto/lorebooks/BusinessObjects/LorebookEntry";

export default function LorebookDetailsComponent() {
  const { activeModule } = sharedContext<SharedContextLorebookType>();
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [isLoadingLorebookDetails, setIsLoadingLorebookDetails] = useState(true);
  const [avatarImageError, setAvatarImageError] = useState(false);
  const [avatarCacheBuster, setAvatarCacheBuster] = useState<number>(Date.now());
  const newAvatarFileInputRef = useRef<HTMLInputElement | null>(null);
  
  // saving state
  const [lorebookResponse, setLorebookResponse] = useState<LorebookResponseDto | null>(null);
  const [lorebookName, setLorebookName] = useState<string>("");
  const [entries, setEntries] = useState<LorebookEntry[]>([]);
  const [isSaving, setIsSaving] = useState(false);
  const [operationError, setOperationError] = useState(false);

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;
    didComponentMountAlready.current = true;

    fetchLorebookDetails();
  }, []);

  const fetchLorebookDetails = async () => {

      if(!activeModule?.selectedLorebookId){
        console.error(`Selected lorebook is not valid and thus has no details.`);
        return;
      }

      try {
        setIsLoadingLorebookDetails(true);
        const response: LorebookResponseDto | null = await getFromServerApiAsync<LorebookResponseDto>(`api/lorebooks/${activeModule.selectedLorebookId}`);
        
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch lorebook details failed. [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setLorebookResponse({
            code : -1,
            lorebook: {
              lastActivityDateTime: null,
              lorebookId: activeModule?.selectedLorebookId,
              name: "",
              entries: [],
            }
          });

          return;
        }
        
        console.log(`Lorebooks details fetched successfully.`);
        setLorebookResponse(response);
        setLorebookName(response?.lorebook?.name ?? "");
        setEntries(response?.lorebook?.entries ?? []);
      } catch (error) {
        console.error("Fetch lorebook error:", error);
      } finally {
        setIsLoadingLorebookDetails(false);
      }
    };

  const handleSave = async () => {
    if (!activeModule?.selectedLorebookId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {
      const payload:LorebookUpdateRequestDto = {
        name: lorebookName,
        entries: entries,
      };
      const response = await putToServerApiAsync(`api/lorebooks/${activeModule.selectedLorebookId}`, payload);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Save failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      }

    } catch (error) {
      console.error("Save lorebook error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!activeModule?.selectedLorebookId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {

      const response = await deleteFromServerApiAsync(`api/lorebooks/${activeModule.selectedLorebookId}`);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Deletion failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      } else{
        let module = {
          moduleName: "lorebooks"
        } as SharedContextType;

        navigateTo(module);
      }

    } catch (error) {
      console.error("Deletion lorebook error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const handleUploadAvatarClick = () => {
    newAvatarFileInputRef.current?.click();
  };

  const handleUploadAvatarFileSelected = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file)
      return;

    const formData = new FormData();
    formData.append("file", file);

    try {
      const response = await postToServerApiAsync<LorebookResponseDto>(`api/lorebooks/${lorebookResponse?.lorebook?.lorebookId}/avatar`, formData);

      let serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code != 200 || serverApiException?.message)
      {
        console.error(`Upload new avatar failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
      }
      
      setAvatarImageError(false);
      setAvatarCacheBuster(Date.now());
      console.log(`Avatar uploaded successfully.`);
    } catch (err) {
      console.error(err);
      // TODO: show err to user
    } finally {
      event.target.value = ""; // reset file input for future uploads
    }
  };

  const handleEntryChange = (index: number, updated: LorebookEntry) => {
    setEntries(prev => prev.map((e, i) => (i === index ? updated : e)));
  };

  return (
    <main className={styles.lorebookDetailsComponent}>
      {isNetworkDown ? (
          <div className={styles.networkDownContainer}>
            <AiOutlineDisconnect className={styles.networkDownIcon} />
            <label>CohesiveRP backend is unreachable</label>
          </div>
        ) : (
        isLoadingLorebookDetails ? (
          <ImSpinner2 className={ styles.loadingLorebookDetailsSpinner } />
        ):(
          <div className={styles.lorebookDetailsContainer}>
            <div className={styles.lorebookHeaderContainer}>
              <div className={styles.lorebookAvatarContainer}>
                {avatarImageError || !lorebookResponse?.lorebook?.lorebookId ? (
                  <div className={styles.addNewAvatarImageContainer} onClick={handleUploadAvatarClick}>
                    <MdAddBox className={styles.addNewAvatarImageBtn} />
                      <input
                        type="file"
                        ref={newAvatarFileInputRef}
                        style={{ display: "none" }}
                        onChange={handleUploadAvatarFileSelected}
                    />
                  </div>
                ) : (
                  <div className={styles.addNewAvatarImageContainer} onClick={handleUploadAvatarClick}>
                    <input
                        type="file"
                        ref={newAvatarFileInputRef}
                        style={{ display: "none" }}
                        onChange={handleUploadAvatarFileSelected}
                    />
                    <img src={`${GetAvatarPathFromLorebookId(lorebookResponse?.lorebook?.lorebookId ?? "")}?t=${avatarCacheBuster}`} alt="no image" onError={() => setAvatarImageError(true)}/>
                  </div>
                )}
              </div>
              <div className={styles.lorebookHeaderRightSideContainer}>
                <textarea
                  className={styles.lorebookName}
                  value={lorebookName}
                  onChange={(e) => setLorebookName(e.target.value)}
                />
                <label className={styles.lorebookId}>{lorebookResponse?.lorebook?.lorebookId ?? ""}</label>
              </div>
            </div>
            <div className={styles.detailsContainer}>
              <div className={styles.lorebookEntriesContainer}>
                <label className={styles.lorebookEntriesLabel}>Entries</label>
                {entries?.length ?? 0 > 0 ? (
                  entries.map((entry, index) => {
                    return (
                      <Fragment key={`Entry_${index}`}>
                        <LorebookEntryComponent
                          entry={entry}
                          onEntryChange={(updated) => handleEntryChange(index, updated)}
                        />
                      </Fragment>
                    );
                  })
                ) : (
                  <p />
                )}
              </div>
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
        )
      )}
    </main>
  );
}