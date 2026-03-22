import styles from "./ChatCompletionPresetsComponent.module.css";
import { useEffect, useRef, useState } from "react";
import { ImSpinner2 } from "react-icons/im";
import { AiOutlineDisconnect } from "react-icons/ai";
import {
  MdAdd,
  MdCheck,
  MdDelete,
  MdKeyboardArrowDown,
  MdKeyboardArrowUp,
  MdDragIndicator,
} from "react-icons/md";

import {
  deleteFromServerApiAsync,
  getFromServerApiAsync,
  postToServerApiAsync,
  putToServerApiAsync,
} from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import {
  ALL_FORMAT_TAGS,
  FORMAT_TAG_LABELS,
} from "../../../../ResponsesDto/chatCompletionPresets/ChatCompletionPresetsResponseDto";
import type {
  ChatCompletionPresetDetailResponseDto,
  ChatCompletionPresetsListResponseDto,
  ChatCompletionPresetSummary,
  PromptContextFormatTag,
  UpsertChatCompletionPresetRequestDto,
} from "../../../../ResponsesDto/chatCompletionPresets/ChatCompletionPresetsResponseDto";
import { generateUUID } from "../../../../utils/uuid";

// ─── Editing element (adds stable key for React during reorder/add/delete) ────

interface EditingElement {
  _key: string;
  tag: PromptContextFormatTag;
  format: string;
}

function toEditingElements(
  raw: { tag: PromptContextFormatTag; options: { format: string } }[]
): EditingElement[] {
  return raw.map(el => ({
    _key: el.tag,
    tag: el.tag,
    format: el.options?.format ?? "",
  }));
}

function fromEditingElements(
  els: EditingElement[]
): { tag: PromptContextFormatTag; options: { format: string } }[] {
  return els.map(el => ({ tag: el.tag, options: { format: el.format } }));
}

// ─── Tag dropdown (single-select) ────────────────────────────────────────────

interface TagDropdownProps {
  value: PromptContextFormatTag;
  onChange: (v: PromptContextFormatTag) => void;
}

function TagDropdown({ value, onChange }: TagDropdownProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  return (
    <div className={styles.tagDropdownWrapper} ref={ref}>
      <button
        className={styles.tagDropdownTrigger}
        type="button"
        onClick={() => setOpen(p => !p)}
      >
        <span className={styles.tagDropdownLabel}>{FORMAT_TAG_LABELS[value]}</span>
        {open
          ? <MdKeyboardArrowUp className={styles.chevron} />
          : <MdKeyboardArrowDown className={styles.chevron} />}
      </button>
      {open && (
        <div className={styles.tagDropdownMenu}>
          {ALL_FORMAT_TAGS.map(tag => {
            const selected = tag === value;
            return (
              <div
                key={tag}
                className={`${styles.tagDropdownItem} ${selected ? styles.tagDropdownItemSelected : ""}`}
                onClick={() => { onChange(tag); setOpen(false); }}
              >
                <span className={styles.checkMark}>
                  {selected && <MdCheck className={styles.checkIcon} />}
                </span>
                <span>{FORMAT_TAG_LABELS[tag]}</span>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

// ─── Single element row ───────────────────────────────────────────────────────

interface ElementRowProps {
  el: EditingElement;
  index: number;
  total: number;
  onChange: (updated: EditingElement) => void;
  onDelete: () => void;
  onMoveUp: () => void;
  onMoveDown: () => void;
}

function ElementRow({ el, index, total, onChange, onDelete, onMoveUp, onMoveDown }: ElementRowProps) {
  return (
    <div className={styles.elementRow}>
      {/* Order controls */}
      <div className={styles.elementOrderControls}>
        <span className={styles.elementIndex}>{index + 1}</span>
        <button
          className={styles.orderBtn}
          type="button"
          disabled={index === 0}
          onClick={onMoveUp}
          title="Move up"
        >
          <MdKeyboardArrowUp className={styles.chevron} />
        </button>
        <button
          className={styles.orderBtn}
          type="button"
          disabled={index === total - 1}
          onClick={onMoveDown}
          title="Move down"
        >
          <MdKeyboardArrowDown className={styles.chevron} />
        </button>
        <MdDragIndicator className={styles.dragHint} title="Order indicator" />
      </div>

      {/* Tag + format */}
      <div className={styles.elementFields}>
        <TagDropdown
          value={el.tag}
          onChange={tag => onChange({ ...el, tag })}
        />
        <textarea
          className={styles.formatTextarea}
          value={el.format}
          placeholder="Optional format / template string…"
          onChange={e => onChange({ ...el, format: e.target.value })}
          rows={2}
        />
      </div>

      {/* Delete */}
      <button
        className={styles.deleteElementBtn}
        type="button"
        onClick={onDelete}
        title="Remove element"
      >
        <MdDelete className={styles.btnDelete} />
      </button>
    </div>
  );
}

// ─── Add-element bar ──────────────────────────────────────────────────────────

interface AddElementBarProps {
  onAdd: (tag: PromptContextFormatTag) => void;
}

function AddElementBar({ onAdd }: AddElementBarProps) {
  const [pickedTag, setPickedTag] = useState<PromptContextFormatTag>("Directive");
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  return (
    <div className={styles.addElementBar}>
      <div className={styles.addElementPickerWrapper} ref={ref}>
        <button
          className={styles.addElementPicker}
          type="button"
          onClick={() => setOpen(p => !p)}
        >
          <span className={styles.tagDropdownLabel}>{FORMAT_TAG_LABELS[pickedTag]}</span>
          {open
            ? <MdKeyboardArrowUp className={styles.chevron} />
            : <MdKeyboardArrowDown className={styles.chevron} />}
        </button>
        {open && (
          <div className={`${styles.tagDropdownMenu} ${styles.addPickerMenu}`}>
            {ALL_FORMAT_TAGS.map(tag => (
              <div
                key={tag}
                className={`${styles.tagDropdownItem} ${tag === pickedTag ? styles.tagDropdownItemSelected : ""}`}
                onClick={() => { setPickedTag(tag); setOpen(false); }}
              >
                <span className={styles.checkMark}>
                  {tag === pickedTag && <MdCheck className={styles.checkIcon} />}
                </span>
                <span>{FORMAT_TAG_LABELS[tag]}</span>
              </div>
            ))}
          </div>
        )}
      </div>
      <button
        className={styles.addElementConfirmBtn}
        type="button"
        onClick={() => onAdd(pickedTag)}
      >
        <MdAdd /> Add Element
      </button>
    </div>
  );
}

// ─── Preset editor (right panel) ─────────────────────────────────────────────

interface PresetEditorProps {
  presetId: string;
  isNew: boolean;
  onSaved: (id: string, name: string) => void;
  onDeleted: (id: string) => void;
  onCancelNew: () => void;
}

function PresetEditor({ presetId, isNew, onSaved, onDeleted, onCancelNew }: PresetEditorProps) {
  const [isLoading, setIsLoading] = useState(!isNew);
  const [loadError, setLoadError] = useState(false);

  const [name, setName] = useState("");
  const [elements, setElements] = useState<EditingElement[]>([]);

  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [saveError, setSaveError] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);

  // Load details when a real preset is selected
  useEffect(() => {
    if (isNew) return;

    let cancelled = false;
    const load = async () => {
      try {
        setIsLoading(true);
        setLoadError(false);
        const response = await getFromServerApiAsync<ChatCompletionPresetDetailResponseDto>(
          `api/chatCompletionPresets/${presetId}`
        );
        const ex = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code !== 200 || ex?.message) {
          console.error("Failed to load preset detail.", JSON.stringify(ex));
          if (!cancelled) setLoadError(true);
          return;
        }
        if (!cancelled) {
          setName(response.chatCompletionPreset.name ?? "");
          setElements(
            toEditingElements(
              response.chatCompletionPreset.format?.orderedElementsWithinTheGlobalPromptContext ?? []
            )
          );
        }
      } catch (e) {
        console.error("Load preset error:", e);
        if (!cancelled) setLoadError(true);
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    };

    load();
    return () => { cancelled = true; };
  }, [presetId, isNew]);

  const updateElement = (index: number, updated: EditingElement) =>
    setElements(prev => prev.map((el, i) => (i === index ? updated : el)));

  const deleteElement = (index: number) =>
    setElements(prev => prev.filter((_, i) => i !== index));

  const moveElement = (index: number, direction: -1 | 1) => {
    setElements(prev => {
      const next = [...prev];
      const target = index + direction;
      if (target < 0 || target >= next.length) return prev;
      [next[index], next[target]] = [next[target], next[index]];
      return next;
    });
  };

  const addElement = (tag: PromptContextFormatTag) =>
    setElements(prev => [...prev, { _key: generateUUID(), tag, format: "" }]);

  const handleSave = async () => {
    if (isSaving || isDeleting) return;
    setSaveError(false);
    setSaveSuccess(false);
    setIsSaving(true);

    const body: UpsertChatCompletionPresetRequestDto = {
      chatCompletionPresetId: presetId,
      name,
      format: {
        orderedElementsWithinTheGlobalPromptContext: fromEditingElements(elements),
      },
    };

    try {
      const response = isNew
        ? await postToServerApiAsync("api/chatCompletionPresets", body)
        : await putToServerApiAsync(`api/chatCompletionPresets/${presetId}`, body);

      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error("Save preset failed.", JSON.stringify(ex));
        setSaveError(true);
      } else {
        setSaveSuccess(true);
        setTimeout(() => setSaveSuccess(false), 5000);
        const savedId = (response as any)?.chatCompletionPresetId ?? presetId;
        onSaved(savedId, name);
      }
    } catch (e) {
      console.error("Save preset error:", e);
      setSaveError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (isSaving || isDeleting || isNew) return;
    setSaveError(false);
    setIsDeleting(true);

    try {
      const response = await deleteFromServerApiAsync(`api/chatCompletionPresets/${presetId}`);
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error("Delete preset failed.", JSON.stringify(ex));
        setSaveError(true);
      } else {
        onDeleted(presetId);
      }
    } catch (e) {
      console.error("Delete preset error:", e);
      setSaveError(true);
    } finally {
      setIsDeleting(false);
    }
  };

  const busy = isSaving || isDeleting;

  // ── Render ─────────────────────────────────────────────────────────────────

  if (isLoading) {
    return (
      <div className={styles.editorCentered}>
        <ImSpinner2 className={styles.spinner} />
      </div>
    );
  }

  if (loadError) {
    return (
      <div className={styles.editorCentered}>
        <AiOutlineDisconnect className={styles.networkIcon} />
        <span>Failed to load preset</span>
      </div>
    );
  }

  return (
    <div className={styles.editor}>
      {/* Header */}
      <div className={styles.editorHeader}>
        <div className={styles.editorHeaderLeft}>
          <input
            className={styles.nameInput}
            value={name}
            placeholder="Preset name…"
            onChange={e => setName(e.target.value)}
          />
          {!isNew && (
            <span className={styles.presetIdLabel}>{presetId}</span>
          )}
        </div>
      </div>

      {/* Elements scroll area */}
      <div className={styles.elementScroll}>
        <div className={styles.elementListHeader}>
          <span className={styles.elementListTitle}>
            Context Format Order
          </span>
          <span className={styles.elementCount}>
            {elements.length} element{elements.length !== 1 ? "s" : ""}
          </span>
        </div>

        {elements.length === 0 && (
          <p className={styles.emptyElements}>
            No elements yet — add one below to build the context format.
          </p>
        )}

        <div className={styles.elementList}>
          {elements.map((el, i) => (
            <ElementRow
              key={el._key}
              el={el}
              index={i}
              total={elements.length}
              onChange={updated => updateElement(i, updated)}
              onDelete={() => deleteElement(i)}
              onMoveUp={() => moveElement(i, -1)}
              onMoveDown={() => moveElement(i, 1)}
            />
          ))}
        </div>

        <AddElementBar onAdd={addElement} />
      </div>

      {/* Footer actions */}
      <div className={styles.editorFooter}>
        <div className={styles.footerLeft}>
          {saveError && <span className={styles.msgError}>Operation failed. Please try again.</span>}
          {saveSuccess && <span className={styles.msgSuccess}>Saved successfully.</span>}
        </div>
        <div className={styles.footerRight}>
          {isNew && (
            <button className={styles.btnGhost} type="button" onClick={onCancelNew} disabled={busy}>
              Cancel
            </button>
          )}
          {!isNew && (
            <button className={styles.btnDanger} type="button" onClick={handleDelete} disabled={busy}>
              {isDeleting ? <ImSpinner2 className={styles.spinnerInline} /> : <><MdDelete /> Delete</>}
            </button>
          )}
          <button className={styles.btnPrimary} type="button" onClick={handleSave} disabled={busy}>
            {isSaving ? <ImSpinner2 className={styles.spinnerInline} /> : <><MdCheck /> Save</>}
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── Main component ───────────────────────────────────────────────────────────

export default function ChatCompletionPresetsComponent() {
  const didMount = useRef(false);

  const [isLoadingList, setIsLoadingList] = useState(true);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [presets, setPresets] = useState<ChatCompletionPresetSummary[]>([]);

  // Which preset is open in the editor; null = nothing selected
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [isCreatingNew, setIsCreatingNew] = useState(false);
  const [pendingNewId, setPendingNewId] = useState<string | null>(null);

  useEffect(() => {
    if (didMount.current) return;
    didMount.current = true;
    fetchList();
  }, []);

  const fetchList = async () => {
    try {
      setIsLoadingList(true);
      const response = await getFromServerApiAsync<ChatCompletionPresetsListResponseDto>(
        "api/chatCompletionPresets"
      );
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code !== 200 || ex?.message) {
        console.error("Failed to fetch presets list.", JSON.stringify(ex));
        setIsNetworkDown(true);
        return;
      }
      setPresets(response.chatCompletionPresets ?? []);
    } catch (e) {
      console.error("Fetch presets list error:", e);
      setIsNetworkDown(true);
    } finally {
      setIsLoadingList(false);
    }
  };

  const handleSelectPreset = (id: string) => {
    setIsCreatingNew(false);
    setPendingNewId(null);
    setSelectedId(id);
  };

  const handleNewPreset = () => {
    const newId = generateUUID();
    setPendingNewId(newId);
    setSelectedId(newId);
    setIsCreatingNew(true);
  };

  const handleSaved = (id: string) => {
    setIsCreatingNew(false);
    setPendingNewId(null);
    setSelectedId(id);
    fetchList();
  };

  const handleDeleted = (id: string) => {
    setPresets(prev => prev.filter(p => p.chatCompletionPresetId !== id));
    setSelectedId(null);
    setIsCreatingNew(false);
    setPendingNewId(null);
  };

  const handleCancelNew = () => {
    setIsCreatingNew(false);
    setPendingNewId(null);
    setSelectedId(null);
  };

  // Active editor ID
  const editorId = isCreatingNew ? pendingNewId : selectedId;

  // ── Render ─────────────────────────────────────────────────────────────────

  return (
    <main className={styles.chatCompletionPresetsComponent}>

      {/* ── Sidebar ──────────────────────────────────────────────────────── */}
      <aside className={styles.sidebar}>
        <div className={styles.sidebarHeader}>
          <span className={styles.sidebarTitle}>Completion Presets</span>
          <button
            className={styles.newBtn}
            type="button"
            onClick={handleNewPreset}
            disabled={isCreatingNew}
            title="New preset"
          >
            <MdAdd className={styles.btnAddPreset} />
          </button>
        </div>

        <div className={styles.sidebarList}>
          {isNetworkDown && (
            <div className={styles.sidebarError}>
              <AiOutlineDisconnect /> Unreachable
            </div>
          )}

          {isLoadingList && (
            <div className={styles.sidebarLoading}>
              <ImSpinner2 className={styles.spinnerInline} />
            </div>
          )}

          {/* "New" item appears at the top while creating */}
          {isCreatingNew && pendingNewId && (
            <div className={`${styles.sidebarItem} ${styles.sidebarItemActive} ${styles.sidebarItemNew}`}>
              <span className={styles.sidebarItemName}>New Preset</span>
            </div>
          )}

          {!isLoadingList && presets.length === 0 && !isCreatingNew && (
            <p className={styles.sidebarEmpty}>No presets yet.</p>
          )}

          {presets.map(preset => (
            <div
              key={preset.chatCompletionPresetId}
              className={`${styles.sidebarItem} ${selectedId === preset.chatCompletionPresetId && !isCreatingNew ? styles.sidebarItemActive : ""}`}
              onClick={() => handleSelectPreset(preset.chatCompletionPresetId)}
            >
              <span className={styles.sidebarItemName}>{preset.name || <em>Unnamed</em>}</span>
            </div>
          ))}
        </div>
      </aside>

      {/* ── Editor panel ─────────────────────────────────────────────────── */}
      <div className={styles.editorPanel}>
        {editorId ? (
          <PresetEditor
            key={editorId}
            presetId={editorId}
            isNew={isCreatingNew}
            onSaved={handleSaved}
            onDeleted={handleDeleted}
            onCancelNew={handleCancelNew}
          />
        ) : (
          <div className={styles.editorEmpty}>
            <span className={styles.editorEmptyIcon}>⚙</span>
            <p>Select a preset to edit, or create a new one.</p>
          </div>
        )}
      </div>

    </main>
  );
}
