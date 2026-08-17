import styles from "./shared/CharacterSheetForm.module.css";
import { useState, useEffect, useRef } from "react";
import { ImSpinner2 } from "react-icons/im";
import { getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { CharacterSheetResponseDto } from "../../../../../ResponsesDto/characters/characterSheets/CharacterSheetResponseDto";
import type { ServerApiResponseDto } from "../../../../../ResponsesDto/ServerApiResponseDto";
import type { CharacterSheetRequestDto } from "../../../../../RequestDto/characters/characterSheets/CharacterSheetRequestDto";
import { useCharacterSheetFields } from "./shared/UseCharacterSheetFields";
import CharacterSheetFormBody from "./shared/CharacterSheetFormBody";

interface Props {
  personaId: string | null;
  characterId: string | null;
}

export default function CharacterSheetComponent({ characterId, personaId }: Props) {
  const didMount = useRef(false);
  const importFileRef = useRef<HTMLInputElement>(null);
  const { sheet, updateField, getAttr, setAttr, getSkill, setSkill, hydrate } = useCharacterSheetFields();

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isRegenerating, setIsRegenerating] = useState(false);
  const [loadError, setLoadError] = useState(false);
  const [saveError, setSaveError] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [regenerateError, setRegenerateError] = useState(false);
  const [characterSheetId, setCharacterSheetId] = useState<string | null>("");

  /* ─── fetch ─── */
  useEffect(() => {
    if (didMount.current) return;
    didMount.current = true;

    const fetchSheet = async () => {
      try {
        let response: CharacterSheetResponseDto | null;
        if (characterId) {
          response = await getFromServerApiAsync<CharacterSheetResponseDto>(`api/characters/${characterId}/characterSheet`);
        } else {
          response = await getFromServerApiAsync<CharacterSheetResponseDto>(`api/characters/personaCharacterSheet/${personaId}`);
        }

        const ex = response as ServerApiExceptionResponseDto | null;
        if (!response || ex?.message) {
          if (ex?.code !== 404) {
            console.error("Failed to load character sheet:", ex);
            setLoadError(true);
            return;
          }
        }

        setCharacterSheetId(response?.characterSheetId ?? null);
        hydrate(response?.characterSheet);
      } catch (err) {
        console.error("Fetch character sheet error:", err);
        setLoadError(true);
      } finally {
        setIsLoading(false);
      }
    };

    fetchSheet();
  }, [characterId]);

  /* ─── save ─── */
  const handleSave = async (): Promise<string | null> => {
    if (isSaving) return null;

    setIsSaving(true);
    setSaveError(false);
    setSaveSuccess(false);

    try {
      const payload: CharacterSheetRequestDto = {
        characterId,
        personaId,
        characterSheetId,
        characterSheet: sheet,
      };

      let response: ServerApiResponseDto | null;
      if (characterSheetId && characterId) {
        response = await putToServerApiAsync(`api/characters/${characterId}/characterSheet/${characterSheetId}`, payload);
      } else if (characterId) {
        response = await postToServerApiAsync(`api/characters/${characterId}/characterSheet`, payload);
      } else if (characterSheetId && personaId) {
        response = await putToServerApiAsync(`api/characters/${personaId}/characterSheet/${characterSheetId}`, payload);
      } else {
        response = await postToServerApiAsync(`api/characters/${personaId}/characterSheet`, payload);
      }

      const ex = response as ServerApiExceptionResponseDto | null;
      const typedResponse = response as CharacterSheetResponseDto;
      if (!response || ex?.message || !typedResponse?.characterSheetId) {
        console.error("Save character sheet failed:", ex);
        setSaveError(true);
      } else {
        setCharacterSheetId(typedResponse.characterSheetId ?? null);
        setSaveSuccess(true);
        setTimeout(() => setSaveSuccess(false), 5000);
        return typedResponse.characterSheetId ?? null;
      }
    } catch (err) {
      console.error("Save character sheet error:", err);
      setSaveError(true);
    } finally {
      setIsSaving(false);
    }

    return null;
  };

  /* ─── regenerate ─── */
  const handleRegenerate = async () => {
    if (isRegenerating || (!characterId && !personaId)) return;

    setIsRegenerating(true);
    setRegenerateError(false);

    try {
      let characterSheetIdToUse = characterSheetId;
      if (!characterSheetIdToUse) {
        characterSheetIdToUse = await handleSave();
      }
      if (!characterSheetIdToUse) {
        console.error(`Couldn't regenerate the CharacterSheet, the operation failed as the CharacterSheetId is null.`);
        return;
      }

      const payload = { characterId, personaId, characterSheetIdToUse };

      let response = null;
      if (characterId) {
        response = await postToServerApiAsync<CharacterSheetResponseDto>(`api/characters/${characterId}/characterSheet/${characterSheetIdToUse}/regenerate`, payload);
      } else {
        response = await postToServerApiAsync<CharacterSheetResponseDto>(`api/characters/personaCharacterSheet/${personaId}/regenerate`, payload);
      }

      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error("Regenerate character sheet failed:", ex);
        setRegenerateError(true);
        return;
      }

      setCharacterSheetId(response.characterSheetId ?? null);
      hydrate(response.characterSheet);
    } catch (err) {
      console.error("Regenerate character sheet error:", err);
      setRegenerateError(true);
    } finally {
      setIsRegenerating(false);
    }
  };

  /* ─── export / import ─── */
  const handleExportJson = () => {
    const blob = new Blob([JSON.stringify(sheet, null, 2)], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    let name = sheet.firstName || "character";
    if (sheet.lastName) name += `_${sheet.lastName}`;
    anchor.download = `${name}_CharacterSheet.json`;
    anchor.click();
    URL.revokeObjectURL(url);
  };

  const handleImportJson = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const parsed = JSON.parse(event.target?.result as string);
        hydrate(parsed.characterSheet ?? parsed);
        e.target.value = "";
      } catch (err) {
        console.error("Failed to parse imported sheet JSON:", err);
        setSaveError(true);
      }
    };
    reader.readAsText(file);
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
        <label>Failed to load character sheet.</label>
      </div>
    );
  }

  return (
    <div className={styles.sheetWrapper}>
      <div className={styles.topActionsBar}>
        <div className={styles.jsonActionsContainer}>
          <button className={styles.jsonActionButton} onClick={handleExportJson} disabled={isSaving || isRegenerating} title="Export character sheet as JSON">
            Export JSON
          </button>
          <button className={styles.jsonActionButton} onClick={() => importFileRef.current?.click()} disabled={isSaving || isRegenerating} title="Import character sheet from JSON and save">
            Import JSON
          </button>
          <input ref={importFileRef} type="file" accept="application/json,.json" className={styles.hiddenFileInput} onChange={handleImportJson} />
        </div>

        <div style={{ display: "flex", gap: "1rem" }}>
          <div className={styles.saveBar}>
            {saveError && <label className={styles.saveErrorLabel}>Failed to save. Please try again.</label>}
            {saveSuccess && <label className={styles.saveSuccessLabel}>Character sheet saved.</label>}
            <button className={styles.saveButton} onClick={handleSave} disabled={isSaving}>
              {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Save Sheet"}
            </button>
          </div>
          <div className={styles.regenerateBar}>
            {regenerateError && <label className={styles.saveErrorLabel}>Regeneration failed. Please try again.</label>}
            <button className={styles.regenerateButton} onClick={handleRegenerate} disabled={isRegenerating} title="Override the whole CharacterSheet with values from querying the LLM">
              {isRegenerating ? <ImSpinner2 className={styles.saveSpinner} /> : "Regenerate Sheet"}
            </button>
          </div>
        </div>
      </div>

      <CharacterSheetFormBody sheet={sheet} updateField={updateField} getAttr={getAttr} setAttr={setAttr} getSkill={getSkill} setSkill={setSkill} />

      <div className={styles.saveBar}>
        {saveError && <label className={styles.saveErrorLabel}>Failed to save. Please try again.</label>}
        {saveSuccess && <label className={styles.saveSuccessLabel}>Character sheet saved.</label>}
        <button className={styles.saveButton} onClick={handleSave} disabled={isSaving}>
          {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Save Sheet"}
        </button>
      </div>
    </div>
  );
}