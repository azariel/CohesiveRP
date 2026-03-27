import styles from "./PersonaDetailsComponent.module.css";
import { useRef, useEffect, useState } from "react";
import { AiOutlineDisconnect } from "react-icons/ai";
import { ImSpinner2 } from "react-icons/im";
import { MdAddBox } from "react-icons/md";

import { deleteFromServerApiAsync, getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextPersonaType } from "../../../../store/SharedContextPersonaType";
import type { PersonaResponseDto } from "../../../../ResponsesDto/personas/PersonaResponseDto";
import { GetAvatarPathFromPersonaId } from "../../../../utils/avatarUtils";
import type { SharedContextType } from "../../../../store/SharedContextType";
import CharacterSheetComponent from "../characterSheets/CharacterSheetComponent";

/* Sub-components */

type ActiveTab = "details" | "characterSheet";

export default function PersonaDetailsComponent() {
  const { activeModule } = sharedContext<SharedContextPersonaType>();
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [isLoadingPersonaDetails, setIsLoadingPersonaDetails] = useState(true);
  const [personaResponse, setPersonaResponse] = useState<PersonaResponseDto | null>(null);
  const [avatarImageError, setAvatarImageError] = useState(false);
  const [avatarCacheBuster, setAvatarCacheBuster] = useState<number>(Date.now());
  const newAvatarFileInputRef = useRef<HTMLInputElement | null>(null);

  // saving state
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const isDefault = useRef(false);
  const [isSaving, setIsSaving] = useState(false);
  const [operationError, setOperationError] = useState(false);

  // tab state
  const [activeTab, setActiveTab] = useState<ActiveTab>("details");

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;
    didComponentMountAlready.current = true;

    fetchPersonaDetails();
  }, []);

  const fetchPersonaDetails = async () => {

      if(!activeModule?.selectedPersonaId){
        console.error(`Selected persona is not valid and thus has no details.`);
        return;
      }

      try {
        setIsLoadingPersonaDetails(true);
        const response: PersonaResponseDto | null = await getFromServerApiAsync<PersonaResponseDto>(`api/personas/${activeModule.selectedPersonaId}`);
        
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch personas details failed. [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setPersonaResponse({
            code : -1,
            persona: null
          });

          return;
        }
        
        console.log(`Personas details fetched successfully.`);
        setPersonaResponse(response);
        setName(response?.persona?.name ?? "");
        setDescription(response?.persona?.description ?? "");
        isDefault.current = response?.persona?.isDefault ?? false;
      } catch (error) {
        console.error("Fetch persona error:", error);
      } finally {
        setIsLoadingPersonaDetails(false);
      }
    };

  const handleSave = async () => {
    if (!activeModule?.selectedPersonaId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {
      const response = await putToServerApiAsync(`api/personas/${activeModule.selectedPersonaId}`,{ description, name, isDefault: isDefault.current });

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Save failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      }

    } catch (error) {
      console.error("Save persona error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!activeModule?.selectedPersonaId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {

      const response = await deleteFromServerApiAsync(`api/personas/${activeModule.selectedPersonaId}`);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Deletion failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      } else{
        let module = {
          moduleName: "personas"
        } as SharedContextType;

        navigateTo(module);
      }

    } catch (error) {
      console.error("Deletion persona error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const handleMakeDefault = async () => {
    if (!activeModule?.selectedPersonaId || isSaving)
      return;

    isDefault.current = true;
    handleSave();
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
      const response = await postToServerApiAsync<PersonaResponseDto>(`api/personas/${personaResponse?.persona?.personaId}/avatar`, formData);

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

  return (
    <main className={styles.personaDetailsComponent}>
      {isNetworkDown ? (
          <div className={styles.networkDownContainer}>
            <AiOutlineDisconnect className={styles.networkDownIcon} />
            <label>CohesiveRP backend is unreachable</label>
          </div>
        ) : (
        isLoadingPersonaDetails ? (
          <ImSpinner2 className={ styles.loadingPersonaDetailsSpinner } />
        ):(
          <div className={styles.personaDetailsContainer}>
            <div className={styles.personaHeaderContainer}>
              <div className={styles.personaAvatarContainer}>
                {avatarImageError || !personaResponse?.persona?.personaId ? (
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
                    <img src={`${GetAvatarPathFromPersonaId(personaResponse?.persona?.personaId ?? "")}?t=${avatarCacheBuster}`} alt="no image" onError={() => setAvatarImageError(true)}/>
                  </div>
                )}
              </div>
              <div className={styles.personaHeaderRightSideContainer}>
                <textarea
                  className={styles.personaName}
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />
                <label className={styles.personaId}>{personaResponse?.persona?.personaId ?? ""}</label>
              </div>
            </div>

            <div className={styles.detailsContainer}>
              {/* ── Tab bar ── */}
              <div className={styles.tabBar}>
                <button
                  className={`${styles.tabButton} ${activeTab === "details" ? styles.tabButtonActive : ""}`}
                  onClick={() => setActiveTab("details")}
                >
                  Details
                </button>
                <button
                  className={`${styles.tabButton} ${activeTab === "characterSheet" ? styles.tabButtonActive : ""}`}
                  onClick={() => setActiveTab("characterSheet")}
                >
                  Character Sheet
                </button>
              </div>

              {/* ── Tab panels ── */}
              {activeTab === "details" && (
                <div className={styles.personaDescriptionContainer}>
                  <label className={styles.personaDescriptionLabel}>Description</label>
                  <textarea
                    className={styles.personaDescription}
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                  />
                  <div className={styles.operationsButtons}>
                    <button className={styles.deleteButton} onClick={handleDelete} disabled={isSaving}>
                    {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Delete"}
                    </button>
                    <button className={styles.saveButton} onClick={handleMakeDefault} disabled={isSaving || isDefault.current}>
                      {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Make Default"}
                    </button>
                    <button className={styles.saveButton} onClick={handleSave} disabled={isSaving}>
                      {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Save"}
                    </button>
                    {operationError && (
                      <label className={styles.saveErrorLabel}>Failed to save/delete. Please try again.</label>
                    )}
                  </div>
                </div>
              )}

              {activeTab === "characterSheet" && personaResponse?.persona?.personaId && (
                <div className={styles.characterSheetTabPanel}>
                  <CharacterSheetComponent personaId={personaResponse.persona.personaId} characterId={null} />
                </div>
              )}
            </div>
          </div>
        )
      )}
    </main>
  );
}
