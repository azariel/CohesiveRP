import { useState } from "react";
import { MdExpandMore, MdDeleteOutline, MdDragIndicator } from "react-icons/md";
import type { LorebookEntry } from "../../../../../ResponsesDto/lorebooks/BusinessObjects/LorebookEntry";
import styles from "./LorebookEntryComponent.module.css";

interface Props {
  entry: LorebookEntry;
  onEntryChange: (updated: LorebookEntry) => void;
  onDelete?: () => void;
  onDragStart?: () => void;
  onDragEnter?: () => void;
  onDrop?: () => void;
  onDragEnd?: () => void;
  isDragging?: boolean;
  isDragOver?: boolean;
}

type Tab = "general" | "trigger" | "advanced";

const LOGIC_GATE_OPTIONS = [
  { value: 0, label: "Main keys only" },
  { value: 1, label: "OR" },
  { value: 2, label: "AND" },
];

const joinList = (list: string[] | null) => (list ?? []).join(", ");
const splitList = (text: string) => text.split(",").map(k => k.trim()).filter(Boolean);

export default function LorebookEntryComponent({
  entry,
  onEntryChange,
  onDelete,
  onDragStart,
  onDragEnter,
  onDrop,
  onDragEnd,
  isDragging,
  isDragOver,
}: Props) {
  // These stay as local buffers (not derived from `entry` on every render) so
  // typing a trailing comma isn't immediately stripped by the round-trip
  // through the parent. Everything else below reads straight from `entry` —
  // it's fully controlled, so parent-driven changes (like insertionOrder
  // renumbering after a drag) always show up correctly.
  const [keysText, setKeysText] = useState(joinList(entry.keys));
  const [secondaryKeysText, setSecondaryKeysText] = useState(joinList(entry.secondaryKeys));
  const [tagsText, setTagsText] = useState(joinList(entry.tags));
  const [activeTab, setActiveTab] = useState<Tab>("general");
  const [canDrag, setCanDrag] = useState(false);

  const [isExpanded, setIsExpanded] = useState(
    !entry.content && !entry.name && (entry.keys ?? []).length === 0
  );

  const notify = (patch: Partial<LorebookEntry>) => {
    onEntryChange({ ...entry, ...patch });
  };

  const displayTitle =
    entry.name?.trim() ||
    keysText.split(",")[0]?.trim() ||
    "Untitled entry";

  return (
    <div
      className={[
        styles.card,
        entry.enabled ? "" : styles.cardDisabled,
        isDragging ? styles.cardDragging : "",
        isDragOver ? styles.cardDragOver : "",
      ].join(" ").trim()}
      draggable={canDrag}
      onDragStart={e => {
        e.dataTransfer.effectAllowed = "move";
        onDragStart?.();
      }}
      onDragEnter={e => { e.preventDefault(); onDragEnter?.(); }}
      onDragOver={e => e.preventDefault()}
      onDrop={e => { e.preventDefault(); onDrop?.(); }}
      onDragEnd={() => { setCanDrag(false); onDragEnd?.(); }}
    >
      <div className={styles.cardHeader} onClick={() => setIsExpanded(prev => !prev)}>
        <span
          className={styles.dragHandle}
          title="Drag to reorder"
          onClick={e => e.stopPropagation()}
          onMouseDown={() => setCanDrag(true)}
          onMouseUp={() => setCanDrag(false)}
        >
          <MdDragIndicator />
        </span>

        <MdExpandMore className={`${styles.chevron} ${isExpanded ? styles.chevronOpen : ""}`} />

        <div className={styles.headerText}>
          <span className={styles.entryTitle}>{displayTitle}</span>
          {keysText && <span className={styles.entryKeysPreview}>{keysText}</span>}
        </div>

        <span className={styles.orderBadge} title="Insertion order">
          #{entry.insertionOrder}
        </span>

        <label className={styles.toggleSwitch} onClick={e => e.stopPropagation()} title="Enabled">
          <input
            type="checkbox"
            checked={entry.enabled}
            onChange={e => notify({ enabled: e.target.checked })}
          />
          <span className={styles.toggleTrack} />
        </label>

        {onDelete && (
          <MdDeleteOutline
            className={styles.deleteIcon}
            title="Remove entry"
            onClick={e => { e.stopPropagation(); onDelete(); }}
          />
        )}
      </div>

      {isExpanded && (
        <div className={styles.cardBody}>
          <div className={styles.tabBar}>
            {(["general", "trigger", "advanced"] as Tab[]).map(tab => (
              <button
                key={tab}
                type="button"
                className={`${styles.tabButton} ${activeTab === tab ? styles.tabButtonActive : ""}`}
                onClick={() => setActiveTab(tab)}
              >
                {tab === "general" ? "General" : tab === "trigger" ? "Trigger" : "Advanced"}
              </button>
            ))}
          </div>

          {activeTab === "general" && (
            <div className={styles.tabPanel}>
              <div className={styles.field}>
                <label className={styles.fieldLabel}>Name</label>
                <input
                  className={styles.textInput}
                  value={entry.name ?? ""}
                  placeholder="Entry name"
                  onChange={e => notify({ name: e.target.value })}
                />
              </div>

              <div className={styles.field}>
                <label className={styles.fieldLabel}>Content</label>
                <textarea
                  className={styles.contentInput}
                  value={entry.content ?? ""}
                  placeholder="What should the AI know when this entry triggers? Note that the entry name is not inserted. This content must be self-contained and make sense on its own."
                  onChange={e => notify({ content: e.target.value })}
                />
              </div>
            </div>
          )}

          {activeTab === "trigger" && (
            <div className={styles.tabPanel}>
              <div className={styles.field}>
                <label className={styles.fieldLabel}>Trigger keys</label>
                <input
                  className={styles.textInput}
                  value={keysText}
                  placeholder="keys, comma-separated"
                  onChange={e => {
                    setKeysText(e.target.value);
                    notify({ keys: splitList(e.target.value) });
                  }}
                />
              </div>

              <div className={styles.field}>
                <label className={styles.fieldLabel}>Secondary keys</label>
                <input
                  className={styles.textInput}
                  value={secondaryKeysText}
                  placeholder="secondary keys, comma-separated"
                  onChange={e => {
                    setSecondaryKeysText(e.target.value);
                    notify({ secondaryKeys: splitList(e.target.value) });
                  }}
                />
              </div>

              <div className={styles.field}>
                <label className={styles.fieldLabel}>Key logic</label>
                <select
                  className={styles.selectInput}
                  value={entry.selectiveLogicBetweenKeysAndSecondaryKeys}
                  onChange={e => notify({ selectiveLogicBetweenKeysAndSecondaryKeys: Number(e.target.value) })}
                >
                  {LOGIC_GATE_OPTIONS.map(opt => (
                    <option key={opt.value} value={opt.value}>{opt.label}</option>
                  ))}
                </select>
              </div>

              <div className={styles.optionsRow}>
                <label className={`${styles.chipToggle} ${entry.constant ? styles.chipToggleActive : ""}`}>
                  <input type="checkbox" checked={entry.constant}
                    onChange={e => notify({ constant: e.target.checked })} />
                  <span>Constant</span>
                </label>
                <label className={`${styles.chipToggle} ${entry.useRegex ? styles.chipToggleActive : ""}`}>
                  <input type="checkbox" checked={entry.useRegex}
                    onChange={e => notify({ useRegex: e.target.checked })} />
                  <span>Use Regex</span>
                </label>
                <label className={`${styles.chipToggle} ${entry.caseSensitive ? styles.chipToggleActive : ""}`}>
                  <input type="checkbox" checked={entry.caseSensitive}
                    onChange={e => notify({ caseSensitive: e.target.checked })} />
                  <span>Case sensitive</span>
                </label>
                <label className={`${styles.chipToggle} ${entry.matchWholeWord ? styles.chipToggleActive : ""}`}>
                  <input type="checkbox" checked={entry.matchWholeWord}
                    onChange={e => notify({ matchWholeWord: e.target.checked })} />
                  <span>Whole word</span>
                </label>
                <label className={`${styles.chipToggle} ${entry.vectorized ? styles.chipToggleActive : ""}`}>
                  <input type="checkbox" checked={entry.vectorized}
                    onChange={e => notify({ vectorized: e.target.checked })} />
                  <span>Vectorized</span>
                </label>
              </div>

              <div className={styles.field}>
                <label className={styles.fieldLabel}>Probability %</label>
                <input
                  type="number" min={0} max={100}
                  className={styles.numberInput}
                  value={entry.probabilityPercentage}
                  onChange={e => notify({ probabilityPercentage: Number(e.target.value) })}
                />
              </div>
            </div>
          )}

          {activeTab === "advanced" && (
            <div className={styles.tabPanel}>
              <div className={styles.numberGrid}>
                <div className={styles.field}>
                  <label className={styles.fieldLabel}>Insertion order</label>
                  <input type="number" className={styles.numberInput}
                    value={entry.insertionOrder}
                    onChange={e => notify({ insertionOrder: Number(e.target.value) })} />
                </div>
                <div className={styles.field}>
                  <label className={styles.fieldLabel}>Position in prompt</label>
                  <input type="number" className={styles.numberInput}
                    value={entry.positionInPrompt}
                    onChange={e => notify({ positionInPrompt: Number(e.target.value) })} />
                </div>
                <div className={styles.field}>
                  <label className={styles.fieldLabel}>Depth</label>
                  <input type="number" className={styles.numberInput}
                    value={entry.depth}
                    onChange={e => notify({ depth: Number(e.target.value) })} />
                </div>
                <div className={styles.field}>
                  <label className={styles.fieldLabel}>Sticky (messages)</label>
                  <input type="number" className={styles.numberInput}
                    value={entry.stickyForNbMessages}
                    onChange={e => notify({ stickyForNbMessages: Number(e.target.value) })} />
                </div>
                <div className={styles.field}>
                  <label className={styles.fieldLabel}>Cooldown</label>
                  <input type="number" className={styles.numberInput}
                    value={entry.cooldown}
                    onChange={e => notify({ cooldown: Number(e.target.value) })} />
                </div>
                <div className={styles.field}>
                  <label className={styles.fieldLabel}>Delay</label>
                  <input type="number" className={styles.numberInput}
                    value={entry.delay}
                    onChange={e => notify({ delay: Number(e.target.value) })} />
                </div>
              </div>

              <div className={styles.optionsRow}>
                <label className={`${styles.chipToggle} ${entry.excludeRecursion ? styles.chipToggleActive : ""}`}>
                  <input type="checkbox" checked={entry.excludeRecursion}
                    onChange={e => notify({ excludeRecursion: e.target.checked })} />
                  <span>Exclude recursion</span>
                </label>
                <label className={`${styles.chipToggle} ${entry.preventRecursion ? styles.chipToggleActive : ""}`}>
                  <input type="checkbox" checked={entry.preventRecursion}
                    onChange={e => notify({ preventRecursion: e.target.checked })} />
                  <span>Prevent recursion</span>
                </label>
                <label className={`${styles.chipToggle} ${entry.onlyTriggeredByRecursion ? styles.chipToggleActive : ""}`}>
                  <input type="checkbox" checked={entry.onlyTriggeredByRecursion}
                    onChange={e => notify({ onlyTriggeredByRecursion: e.target.checked })} />
                  <span>Only via recursion</span>
                </label>
                <label className={`${styles.chipToggle} ${entry.ignoreTokensBudget ? styles.chipToggleActive : ""}`}>
                  <input type="checkbox" checked={entry.ignoreTokensBudget}
                    onChange={e => notify({ ignoreTokensBudget: e.target.checked })} />
                  <span>Ignore token budget</span>
                </label>
              </div>

              <div className={styles.field}>
                <label className={styles.fieldLabel}>Tags</label>
                <input
                  className={styles.textInput}
                  value={tagsText}
                  placeholder="tags, comma-separated"
                  onChange={e => {
                    setTagsText(e.target.value);
                    notify({ tags: splitList(e.target.value) });
                  }}
                />
              </div>

              <div className={styles.field}>
                <label className={styles.fieldLabel}>Comment</label>
                <textarea
                  className={styles.commentInput}
                  value={entry.comment ?? ""}
                  placeholder="Internal note — not sent to the AI"
                  onChange={e => notify({ comment: e.target.value })}
                />
              </div>

              {entry.entryId && (
                <div className={styles.entryIdRow}>Entry ID: {entry.entryId}</div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}