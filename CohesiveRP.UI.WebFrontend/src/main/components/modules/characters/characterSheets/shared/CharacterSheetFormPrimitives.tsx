import styles from "./CharacterSheetForm.module.css";
import { useState, useEffect } from "react";
import type { CharacterStatusEffect } from "../../../../../../ResponsesDto/characters/characterSheets/CharacterStatusEffect";

export const GENDER_OPTIONS = ["", "Male", "Female"];
export const AGE_GROUP_OPTIONS = ["", "Infant", "Toddler", "Children", "Teenager", "YoungAdult", "Adult", "Elderly"];
export const GENITALS_OPTIONS = ["", "Male", "Female", "Both", "None"];
export const BREASTS_SIZE_OPTIONS = ["", "Flat", "Small", "Average", "Large", "ExtraLarge", "Enormous"];

const arrToText = (arr?: string[]) => (arr ?? []).join("\n");
const textToArr = (text: string) => text.split("\n").map((s) => s.trim()).filter(Boolean);

export function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className={styles.section}>
      <h3 className={styles.sectionTitle}>{title}</h3>
      <div className={styles.sectionBody}>{children}</div>
    </div>
  );
}

export function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className={styles.field}>
      <label className={styles.fieldLabel}>{label}</label>
      {children}
    </div>
  );
}

export function SheetInput({ value, onChange, placeholder }: { value: string; onChange: (v: string) => void; placeholder?: string; }) {
  return (
    <input
      className={styles.sheetInput}
      value={value ?? ""}
      onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
    />
  );
}

export function SheetTextarea({ value, onChange, minHeight = "5em" }: { value: string; onChange: (v: string) => void; minHeight?: string; }) {
  return (
    <textarea
      className={styles.sheetTextarea}
      style={{ minHeight }}
      value={value ?? ""}
      onChange={(e) => onChange(e.target.value)}
    />
  );
}

export function SheetSelect({ value, onChange, options }: { value: string; onChange: (v: string) => void; options: string[]; }) {
  return (
    <select className={styles.sheetSelect} value={value ?? ""} onChange={(e) => onChange(e.target.value)}>
      {options.map((opt) => (
        <option key={opt} value={opt}>{opt || "— none —"}</option>
      ))}
    </select>
  );
}

export function SheetNumberInput({ value, onChange, min = 0, max = 100 }: { value: number; onChange: (v: number) => void; min?: number; max?: number; }) {
  return (
    <input
      type="number"
      className={styles.sheetInput}
      value={value ?? 0}
      min={min}
      max={max}
      onChange={(e) => onChange(Number(e.target.value))}
    />
  );
}

export function StatusEffectListField({ label, value, onChange }: { label: string; value: CharacterStatusEffect[]; onChange: (v: CharacterStatusEffect[]) => void; }) {
  const handleContentChange = (index: number, content: string) => {
    const next = [...value];
    next[index] = { ...next[index], content };
    onChange(next);
  };
  const handleExpiresAtChange = (index: number, expiresAt: string) => {
    const next = [...value];
    next[index] = { ...next[index], expiresAt };
    onChange(next);
  };
  const handleRemove = (index: number) => onChange(value.filter((_, i) => i !== index));
  const handleAdd = () => onChange([...value, { content: "", expiresAt: "SEMI-PERMANENT" }]);

  return (
    <Field label={label}>
      <div className={styles.statusEffectList}>
        {value.length === 0 && <span className={styles.statusEffectEmpty}>None currently active.</span>}
        {value.map((effect, index) => (
          <div key={index} className={styles.statusEffectRow}>
            <textarea
              className={styles.sheetTextarea}
              style={{ minHeight: "3.5em" }}
              value={effect.content ?? ""}
              onChange={(e) => handleContentChange(index, e.target.value)}
              placeholder="e.g. Deep laceration on left forearm"
            />
            <div className={styles.statusEffectMeta}>
              <input
                className={styles.sheetInput}
                value={effect.expiresAt ?? ""}
                onChange={(e) => handleExpiresAtChange(index, e.target.value)}
                placeholder="PERMANENT, SEMI-PERMANENT, UNKNOWN or exact datetime"
              />
              <button type="button" className={styles.statusEffectRemoveBtn} onClick={() => handleRemove(index)} title="Remove entry">
                Remove
              </button>
            </div>
          </div>
        ))}
        <button type="button" className={styles.statusEffectAddBtn} onClick={handleAdd}>+ Add entry</button>
      </div>
    </Field>
  );
}

export function ArrayField({ label, value, onChange, minHeight = "6em" }: { label: string; value: string[]; onChange: (v: string[]) => void; minHeight?: string; }) {
  const [localText, setLocalText] = useState(() => arrToText(value));

  useEffect(() => {
    setLocalText((prevText) => {
      const parsedLocal = textToArr(prevText);
      if (JSON.stringify(parsedLocal) !== JSON.stringify(value)) return arrToText(value);
      return prevText;
    });
  }, [value]);

  const handleTextChange = (v: string) => {
    setLocalText(v);
    onChange(textToArr(v));
  };

  return (
    <Field label={`${label} (one per line)`}>
      <SheetTextarea value={localText} onChange={handleTextChange} minHeight={minHeight} />
    </Field>
  );
}