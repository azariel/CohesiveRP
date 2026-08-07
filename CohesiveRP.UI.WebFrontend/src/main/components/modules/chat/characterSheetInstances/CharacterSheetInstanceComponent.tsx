import styles from "../../characters/characterSheets/shared/CharacterSheetForm.module.css";
import { useState, useEffect, useRef } from "react";
import { ImSpinner2 } from "react-icons/im";
import { MdArrowBack } from "react-icons/md";
import { getFromServerApiAsync, putToServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { CharacterSheetInstanceResponseDto } from "../../../../../ResponsesDto/characters/characterSheetInstances/CharacterSheetInstanceResponseDto";
import type { CharacterSheetInstanceRequestDto } from "../../../../../RequestDto/characters/characterSheetInstances/CharacterSheetInstanceRequestDto";
import { useCharacterSheetFields } from "../../characters/characterSheets/shared/UseCharacterSheetFields";
import CharacterSheetFormBody from "../../characters/characterSheets/shared/CharacterSheetFormBody";

interface Props {
  characterId: string;
  chatId: string;
  characterName?: string;
  onBack: () => void;
}

export default function CharacterSheetInstanceComponent({ characterId, chatId, characterName, onBack }: Props) {
  const didMount = useRef(false);
  const { sheet, updateField, getAttr, setAttr, getSkill, setSkill, hydrate } = useCharacterSheetFields();

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [loadError, setLoadError] = useState(false);
  const [saveError, setSaveError] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [characterSheetInstanceId, setCharacterSheetInstanceId] = useState<string | null>(null);

  /* ─── fetch ─── */
  useEffect(() => {
    if (didMount.current) return;
    didMount.current = true;

    const fetchInstance = async () => {
      try {
        const response = await getFromServerApiAsync<CharacterSheetInstanceResponseDto>(
          `api/characters/${characterId}/chats/${chatId}/characterSheetInstance`
        );

        const ex = response as ServerApiExceptionResponseDto | null;
        if (!response || ex?.message) {
          console.error("Failed to load character sheet instance:", ex);
          setLoadError(true);
          return;
        }

        setCharacterSheetInstanceId(response.characterSheetInstanceId ?? null);
        hydrate(response.characterSheet);
      } catch (err) {
        console.error("Fetch character sheet instance error:", err);
        setLoadError(true);
      } finally {
        setIsLoading(false);
      }
    };

    fetchInstance();
  }, [characterId, chatId]);

  /* ─── save ─── */
  const handleSave = async () => {
    if (isSaving || !characterSheetInstanceId) return;

    setIsSaving(true);
    setSaveError(false);
    setSaveSuccess(false);

    try {
      const payload: CharacterSheetInstanceRequestDto = {
        characterId,
        chatId,
        characterSheetInstanceId,
        characterSheet: sheet,
      };

      const response = await putToServerApiAsync(
        `api/characters/${characterId}/chats/${chatId}/characterSheetInstances/${characterSheetInstanceId}`,
        payload
      );

      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error("Save character sheet instance failed:", ex);
        setSaveError(true);
      } else {
        setSaveSuccess(true);
        setTimeout(() => setSaveSuccess(false), 5000);
      }
    } catch (err) {
      console.error("Save character sheet instance error:", err);
      setSaveError(true);
    } finally {
      setIsSaving(false);
    }
  };

  /* ─── render ─── */
  if (isLoading) {
    return (
      <div className={styles.loadingContainer}>
        <ImSpinner2 className={styles.spinner} />
      </div>
    );
  }

  if (loadError) {
    return (
      <div className={styles.errorContainer}>
        <label>Failed to load character sheet instance.</label>
        <button className={styles.backButton} onClick={onBack}>
          <MdArrowBack /> Back
        </button>
      </div>
    );
  }

  return (
    <div className={styles.sheetWrapper}>
      <div className={styles.topActionsBar}>
        <button className={styles.backButton} onClick={onBack} title="Back to character list">
          <MdArrowBack /> Back
        </button>

        <div className={styles.headerLabelBlock}>
          <label className={styles.instanceHeaderLabel}>
            {characterName ?? sheet.firstName} — Chat Instance
          </label>
        </div>

        <div className={styles.saveBar}>
          {saveError && <label className={styles.saveErrorLabel}>Failed to save. Please try again.</label>}
          {saveSuccess && <label className={styles.saveSuccessLabel}>Instance saved.</label>}
          <button className={styles.saveButton} onClick={handleSave} disabled={isSaving}>
            {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Save Instance"}
          </button>
        </div>
      </div>

      <CharacterSheetFormBody sheet={sheet} updateField={updateField} getAttr={getAttr} setAttr={setAttr} getSkill={getSkill} setSkill={setSkill} />

      <div className={styles.saveBar}>
        {saveError && <label className={styles.saveErrorLabel}>Failed to save. Please try again.</label>}
        {saveSuccess && <label className={styles.saveSuccessLabel}>Instance saved.</label>}
        <button className={styles.saveButton} onClick={handleSave} disabled={isSaving}>
          {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Save Instance"}
        </button>
      </div>
    </div>
  );
}