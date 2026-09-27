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

interface KeyedEntry {
  clientKey: string;
  entry: LorebookEntry;
}

const generateClientKey = (): string =>
  (typeof crypto !== "undefined" && "randomUUID" in crypto)
    ? crypto.randomUUID()
    : `entry-${Date.now()}-${Math.random().toString(36).slice(2)}`;

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
  const [keyedEntries, setKeyedEntries] = useState<KeyedEntry[]>([]);
  const [isSaving, setIsSaving] = useState(false);
  const [operationError, setOperationError] = useState(false);

  // drag-and-drop reorder state
  const [draggedIndex, setDraggedIndex] = useState<number | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);

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
        setKeyedEntries(
          (response?.lorebook?.entries ?? []).map(entry => ({
            clientKey: generateClientKey(),
            entry,
          }))
        );
      } catch (error) {
        console.error("Fetch lorebook error:", error);
      } finally {
        setIsLoadingLorebookDetails(false);
      }
    };

    const createEmptyLorebookEntry = (nextInsertionOrder: number): LorebookEntry => ({
      keys: [],
      content: "",
      enabled: true,
      insertionOrder: nextInsertionOrder,
      useRegex: false,
      constant: false,
      depth: 0,
      caseSensitive: false,
      comment: "",
      secondaryKeys: [],
      vectorized: false,
      matchWholeWord: false,
      probabilityPercentage: 100,
      positionInPrompt: 0,
      stickyForNbMessages: 0,
      cooldown: 0,
      ignoreTokensBudget: false,
      delay: 0,
      excludeRecursion: false,
      preventRecursion: false,
      onlyTriggeredByRecursion: false,
      tags: [],
      name: "",
      entryId: "",
      selectiveLogicBetweenKeysAndSecondaryKeys: 0, // KeysEvaluationLogicGate.MainKeysOnly
    });

  const handleSave = async () => {
    if (!activeModule?.selectedLorebookId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {
      const payload:LorebookUpdateRequestDto = {
        name: lorebookName,
        entries: keyedEntries.map(k => k.entry),
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
    setKeyedEntries(prev => prev.map((k, i) => (i === index ? { ...k, entry: updated } : k)));
  };

  const handleAddEntry = () => {
    setKeyedEntries((prev) => {
      const nextOrder = prev.length > 0
        ? Math.max(...prev.map(k => k.entry.insertionOrder ?? 0)) + 1
        : 0;
      return [{ clientKey: generateClientKey(), entry: createEmptyLorebookEntry(nextOrder) }, ...prev];
    });
  };

  const handleRemoveEntry = (index: number) => {
    setKeyedEntries(prev => prev.filter((_, i) => i !== index));
  };

  const handleDragStart = (index: number) => setDraggedIndex(index);

  const handleDragEnter = (index: number) => {
    if (draggedIndex === null || draggedIndex === index) return;
    setDragOverIndex(index);
  };

  // The move happens on drop; insertionOrder is renumbered to match the new
  // visual order so what gets saved actually matches what you dragged.
  const handleDrop = (targetIndex: number) => {
    setKeyedEntries(prev => {
      if (draggedIndex === null || draggedIndex === targetIndex) return prev;
      const updated = [...prev];
      const [moved] = updated.splice(draggedIndex, 1);
      updated.splice(targetIndex, 0, moved);
      return updated.map((k, i) => ({ ...k, entry: { ...k.entry, insertionOrder: i } }));
    });
    setDraggedIndex(null);
    setDragOverIndex(null);
  };

  const handleDragEnd = () => {
    setDraggedIndex(null);
    setDragOverIndex(null);
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
                <div className={styles.lorebookEntriesHeader}>
                  <div className={styles.lorebookEntriesTitleGroup}>
                    <span className={styles.lorebookEntriesAccent} />
                    <div className={styles.lorebookEntriesTitleText}>
                      <label className={styles.lorebookEntriesLabel}>Entries</label>
                      <span className={styles.lorebookEntriesCount}>
                        {keyedEntries.length} {keyedEntries.length === 1 ? "entry" : "entries"}
                      </span>
                    </div>
                  </div>

                  <button type="button" className={styles.addEntryButton} onClick={handleAddEntry}>
                    <MdAddBox className={styles.addEntryButtonIcon} />
                    <span>New entry</span>
                  </button>
                </div>
                {keyedEntries.length > 0 ? (
                  keyedEntries.map((keyed, index) => (
                    <Fragment key={keyed.clientKey}>
                      <LorebookEntryComponent
                        entry={keyed.entry}
                        onEntryChange={(updated) => handleEntryChange(index, updated)}
                        onDelete={() => handleRemoveEntry(index)}
                        onDragStart={() => handleDragStart(index)}
                        onDragEnter={() => handleDragEnter(index)}
                        onDrop={() => handleDrop(index)}
                        onDragEnd={handleDragEnd}
                        isDragging={draggedIndex === index}
                        isDragOver={dragOverIndex === index && draggedIndex !== index}
                      />
                    </Fragment>
                  ))
                ) : (
                  <p />
                )}
              </div>
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
        )
      )}
    </main>
  );
}