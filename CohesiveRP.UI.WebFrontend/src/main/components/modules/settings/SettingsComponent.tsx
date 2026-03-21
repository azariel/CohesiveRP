import styles from "./SettingsComponent.module.css";
import { useEffect, useState, useRef } from "react";
import { ImSpinner2 } from "react-icons/im";
import { AiOutlineDisconnect } from "react-icons/ai";
import { MdAdd, MdCheck, MdDelete, MdEdit, MdKeyboardArrowDown, MdKeyboardArrowUp } from "react-icons/md";

import { getFromServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { SettingsResponseDto } from "../../../../ResponsesDto/settings/SettingsResponseDto";
import type { LlmProviderType, PriorityType, TagType, TimeoutStrategyType } from "../../../../ResponsesDto/settings/BusinessObjects/SettingsEnums";
import type { TimeoutStrategy } from "../../../../ResponsesDto/settings/BusinessObjects/TimeoutStrategy";
import type { ChatCompletionPreset } from "../../../../ResponsesDto/chatCompletionPresets/ChatCompletionPreset";
import type { ChatCompletionPresetsResponseDto } from "../../../../ResponsesDto/chatCompletionPresets/ChatCompletionPresetsResponseDto";
import type { SummarySettings, ExtensibleSummaryConfig, OverflowSummaryConfig } from "../../../../ResponsesDto/settings/BusinessObjects/SummarySettings";
import type { LLMProviderSettings } from "../../../../ResponsesDto/settings/BusinessObjects/LLMProviderSettings";
import type { ChatCompletionPresetMapEntry } from "../../../../ResponsesDto/settings/BusinessObjects/ChatCompletionPresetsMap";

// ─── Constants ────────────────────────────────────────────────────────────────

const ALL_TAGS: TagType[] = ["Main", "Summarize", "SummariesMerge", "SceneTracker"];
const PROVIDER_TYPES: LlmProviderType[] = ["OpenAICustom"];
const TIMEOUT_STRATEGY_TYPES: TimeoutStrategyType[] = ["RetryXtimesThenGiveUp"];
const PRIORITY_TYPES: PriorityType[] = ["Standard", "High", "Low"];

const DEFAULT_SUMMARY: SummarySettings = {
  shortConfig:    { nbMessageInChunk: 3, maxShortTermSummaryTokens: 1024 },
  mediumConfig:   { summarizeLastXTokens: 256, maxTotalSummariesTokens: 1024 },
  longConfig:     { summarizeLastXTokens: 256, maxTotalSummariesTokens: 1024 },
  extraConfig:    { summarizeLastXTokens: 256, maxTotalSummariesTokens: 1024 },
  overflowConfig: { summarizeLastXTokens: 256, maxOverflowSummaryTokens: 1024 },
  nbRawMessagesToKeepInContext: 5,
  hotMessagesAmountLimit:      30,
  coldMessagesAmountLimit:     200,
};

function newProvider(): LLMProviderSettings {
  return {
    providerConfigId: "",
    name: "",
    apiUrl: "",
    type: "OpenAICustom",
    concurrencyLimit: 1,
    model: "",
    priority: "Standard",
    tags: [],
    timeoutStrategy: { type: "RetryXtimesThenGiveUp", retries: 3 },
  };
}

// ─── Reusable: single-select dropdown ────────────────────────────────────────

interface SelectDropdownProps<T extends string> {
  value: T;
  options: T[];
  onChange: (v: T) => void;
  disabled?: boolean;
}

function SelectDropdown<T extends string>({ value, options, onChange, disabled }: SelectDropdownProps<T>) {
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
    <div className={styles.dropdownWrapper} ref={ref}>
      <button
        className={styles.dropdownTrigger}
        type="button"
        disabled={disabled}
        onClick={() => setOpen(p => !p)}
      >
        <span className={styles.dropdownLabel}>{value}</span>
        {open
          ? <MdKeyboardArrowUp className={styles.dropdownChevron} />
          : <MdKeyboardArrowDown className={styles.dropdownChevron} />}
      </button>
      {open && (
        <div className={styles.dropdownMenu}>
          {options.map(opt => (
            <div
              key={opt}
              className={`${styles.dropdownItem} ${opt === value ? styles.dropdownItemSelected : ""}`}
              onClick={() => { onChange(opt); setOpen(false); }}
            >
              <span className={styles.checkMark}>{opt === value && <MdCheck className={styles.checkIcon} />}</span>
              <span>{opt}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// ─── Reusable: multi-select tags dropdown ─────────────────────────────────────

interface TagsDropdownProps {
  selected: TagType[];
  onChange: (tags: TagType[]) => void;
}

function TagsDropdown({ selected, onChange }: TagsDropdownProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  const toggle = (tag: TagType) =>
    onChange(selected.includes(tag) ? selected.filter(t => t !== tag) : [...selected, tag]);

  const label = selected.length === 0 ? "None" : selected.join(", ");

  return (
    <div className={styles.dropdownWrapper} ref={ref}>
      <button className={styles.dropdownTrigger} type="button" onClick={() => setOpen(p => !p)}>
        <span className={styles.dropdownLabel}>{label}</span>
        {open
          ? <MdKeyboardArrowUp className={styles.dropdownChevron} />
          : <MdKeyboardArrowDown className={styles.dropdownChevron} />}
      </button>
      {open && (
        <div className={styles.dropdownMenu}>
          {ALL_TAGS.map(tag => {
            const isSelected = selected.includes(tag);
            return (
              <div
                key={tag}
                className={`${styles.dropdownItem} ${isSelected ? styles.dropdownItemSelected : ""}`}
                onClick={() => toggle(tag)}
              >
                <span className={styles.checkMark}>{isSelected && <MdCheck className={styles.checkIcon} />}</span>
                <span>{tag}</span>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

// ─── LLM Provider Editor ──────────────────────────────────────────────────────

interface ProviderEditorProps {
  initial: LLMProviderSettings;
  isNew: boolean;
  onApply: (p: LLMProviderSettings) => void;
  onCancel: () => void;
  onDelete?: () => void;
}

function ProviderEditor({ initial, isNew, onApply, onCancel, onDelete }: ProviderEditorProps) {
  const [p, setP] = useState<LLMProviderSettings>({ ...initial, tags: [...initial.tags] });

  const set = <K extends keyof LLMProviderSettings>(key: K, value: LLMProviderSettings[K]) =>
    setP(prev => ({ ...prev, [key]: value }));

  const setTO = <K extends keyof TimeoutStrategy>(key: K, value: TimeoutStrategy[K]) =>
    setP(prev => ({ ...prev, timeoutStrategy: { ...prev.timeoutStrategy, [key]: value } }));

  return (
    <div className={styles.editor}>
      <div className={styles.editorGrid}>

        <label className={styles.fieldLabel}>Name</label>
        <input
          className={styles.fieldInput}
          value={p.name}
          placeholder="e.g. My Local LLM"
          onChange={e => set("name", e.target.value)}
        />

        <label className={styles.fieldLabel}>API URL</label>
        <input
          className={styles.fieldInput}
          value={p.apiUrl}
          placeholder="http://127.0.0.1:7777/v1/chat/completions"
          onChange={e => set("apiUrl", e.target.value)}
        />

        <label className={styles.fieldLabel}>Model</label>
        <input
          className={styles.fieldInput}
          value={p.model}
          placeholder="e.g. gpt-4o"
          onChange={e => set("model", e.target.value)}
        />

        <label className={styles.fieldLabel}>Type</label>
        <SelectDropdown value={p.type} options={PROVIDER_TYPES} onChange={v => set("type", v)} />

        <label className={styles.fieldLabel}>Priority</label>
        <SelectDropdown value={p.priority} options={PRIORITY_TYPES} onChange={v => set("priority", v)} />

        <label className={styles.fieldLabel}>Concurrency</label>
        <input
          className={`${styles.fieldInput} ${styles.fieldInputNarrow}`}
          type="number"
          min={1}
          max={99}
          value={p.concurrencyLimit}
          onChange={e => set("concurrencyLimit", Math.max(1, parseInt(e.target.value) || 1))}
        />

        <label className={styles.fieldLabel}>Tags</label>
        <TagsDropdown selected={p.tags} onChange={tags => set("tags", tags)} />

        <label className={styles.fieldLabel}>On Timeout</label>
        <SelectDropdown
          value={p.timeoutStrategy.type}
          options={TIMEOUT_STRATEGY_TYPES}
          onChange={v => setTO("type", v)}
        />

        <label className={styles.fieldLabel}>Retries</label>
        <input
          className={`${styles.fieldInput} ${styles.fieldInputNarrow}`}
          type="number"
          min={0}
          max={99}
          value={p.timeoutStrategy.retries}
          onChange={e => setTO("retries", Math.max(0, parseInt(e.target.value) || 0))}
        />

      </div>

      <div className={styles.editorActions}>
        {!isNew && onDelete && (
          <button className={styles.btnDanger} type="button" onClick={onDelete}>
            <MdDelete /> Delete
          </button>
        )}
        <span className={styles.spacer} />
        <button className={styles.btnGhost} type="button" onClick={onCancel}>Cancel</button>
        <button className={styles.btnPrimary} type="button" onClick={() => onApply(p)}>
          <MdCheck /> Apply
        </button>
      </div>
    </div>
  );
}

// ─── Provider Card (collapsed) ────────────────────────────────────────────────

interface ProviderCardProps {
  provider: LLMProviderSettings;
  editingDisabled: boolean;
  onEdit: () => void;
}

function ProviderCard({ provider, editingDisabled, onEdit }: ProviderCardProps) {
  return (
    <div className={styles.cardCollapsed}>
      <div className={styles.cardInfo}>
        <span className={styles.cardName}>{provider.name || <em>Unnamed</em>}</span>
        <span className={styles.pill}>{provider.type}</span>
        <span className={styles.pill}>{provider.model || "—"}</span>
        {provider.tags.map(tag => (
          <span key={tag} className={`${styles.pill} ${styles.pillAccent}`}>{tag}</span>
        ))}
      </div>
      <button
        className={styles.iconBtn}
        type="button"
        disabled={editingDisabled}
        onClick={onEdit}
        title="Edit provider"
      >
        <MdEdit className={styles.editIcon} />
      </button>
    </div>
  );
}

// ─── Preset Map Row ───────────────────────────────────────────────────────────

interface PresetRowProps {
  tag: TagType;
  selectedId: string;
  presets: ChatCompletionPreset[];
  isLoading: boolean;
  onChange: (id: string) => void;
}

function PresetRow({ tag, selectedId, presets, isLoading, onChange }: PresetRowProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  const label = presets.find(p => p.chatCompletionPresetId === selectedId)?.name ?? "None selected";

  return (
    <div className={styles.presetRow}>
      <span className={styles.presetTag}>{tag}</span>
      <div className={styles.dropdownWrapper} ref={ref}>
        <button
          className={styles.dropdownTrigger}
          type="button"
          disabled={isLoading}
          onClick={() => setOpen(p => !p)}
        >
          {isLoading
            ? <ImSpinner2 className={styles.spinnerInline} />
            : <span className={styles.dropdownLabel}>{label}</span>}
          {open
            ? <MdKeyboardArrowUp className={styles.dropdownChevron} />
            : <MdKeyboardArrowDown className={styles.dropdownChevron} />}
        </button>
        {open && (
          <div className={styles.dropdownMenu}>
            <div
              className={`${styles.dropdownItem} ${!selectedId ? styles.dropdownItemSelected : ""}`}
              onClick={() => { onChange(""); setOpen(false); }}
            >
              <span className={styles.checkMark}>{!selectedId && <MdCheck className={styles.checkIcon} />}</span>
              <span className={styles.noneOption}>None</span>
            </div>
            {presets.map(preset => {
              const selected = preset.chatCompletionPresetId === selectedId;
              return (
                <div
                  key={preset.chatCompletionPresetId}
                  className={`${styles.dropdownItem} ${selected ? styles.dropdownItemSelected : ""}`}
                  onClick={() => { onChange(preset.chatCompletionPresetId); setOpen(false); }}
                >
                  <span className={styles.checkMark}>{selected && <MdCheck className={styles.checkIcon} />}</span>
                  <span>{preset.name}</span>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}

// ─── Summary Section ──────────────────────────────────────────────────────────

interface SummarySectionProps {
  summary: SummarySettings;
  onChange: (s: SummarySettings) => void;
}

function NumField({
  label, value, onChange, min = 0,
}: { label: string; value: number; onChange: (n: number) => void; min?: number }) {
  return (
    <>
      <label className={styles.fieldLabel}>{label}</label>
      <input
        className={`${styles.fieldInput} ${styles.fieldInputNarrow}`}
        type="number"
        min={min}
        value={value}
        onChange={e => onChange(Math.max(min, parseInt(e.target.value) || min))}
      />
    </>
  );
}

function SummarySection({ summary, onChange }: SummarySectionProps) {
  const setTop = <K extends keyof SummarySettings>(key: K, value: SummarySettings[K]) =>
    onChange({ ...summary, [key]: value });

  const setShort = (key: keyof SummarySettings["shortConfig"], value: number) =>
    setTop("shortConfig", { ...summary.shortConfig, [key]: value });

  const setExtensible = (
    which: "mediumConfig" | "longConfig" | "extraConfig",
    key: keyof ExtensibleSummaryConfig,
    value: number,
  ) => setTop(which, { ...summary[which], [key]: value });

  const setOverflow = (key: keyof OverflowSummaryConfig, value: number) =>
    setTop("overflowConfig", { ...summary.overflowConfig, [key]: value });

  const SubCard = ({ title, children }: { title: string; children: React.ReactNode }) => (
    <div className={styles.summarySubCard}>
      <div className={styles.summarySubCardHeader}>{title}</div>
      <div className={styles.editorGrid}>{children}</div>
    </div>
  );

  return (
    <div className={styles.summaryGrid}>

      {/* Global knobs */}
      <div className={styles.summarySubCard}>
        <div className={styles.summarySubCardHeader}>Global</div>
        <div className={styles.editorGrid}>
          <NumField label="Raw messages in context" value={summary.nbRawMessagesToKeepInContext}
            onChange={v => setTop("nbRawMessagesToKeepInContext", v)} min={1} />
          <NumField label="Hot messages limit" value={summary.hotMessagesAmountLimit}
            onChange={v => setTop("hotMessagesAmountLimit", v)} min={1} />
          <NumField label="Cold messages limit" value={summary.coldMessagesAmountLimit}
            onChange={v => setTop("coldMessagesAmountLimit", v)} min={1} />
        </div>
      </div>

      {/* Short */}
      <SubCard title="Short">
        <NumField label="Messages per chunk" value={summary.shortConfig.nbMessageInChunk}
          onChange={v => setShort("nbMessageInChunk", v)} min={1} />
        <NumField label="Max tokens" value={summary.shortConfig.maxShortTermSummaryTokens}
          onChange={v => setShort("maxShortTermSummaryTokens", v)} min={1} />
      </SubCard>

      {/* Medium / Long / Extra share the same shape */}
      {(["mediumConfig", "longConfig", "extraConfig"] as const).map(key => (
        <SubCard key={key} title={key === "mediumConfig" ? "Medium" : key === "longConfig" ? "Long" : "Extra"}>
          <NumField label="Summarize last X tokens" value={summary[key].summarizeLastXTokens}
            onChange={v => setExtensible(key, "summarizeLastXTokens", v)} min={1} />
          <NumField label="Max total tokens" value={summary[key].maxTotalSummariesTokens}
            onChange={v => setExtensible(key, "maxTotalSummariesTokens", v)} min={1} />
        </SubCard>
      ))}

      {/* Overflow */}
      <SubCard title="Overflow">
        <NumField label="Summarize last X tokens" value={summary.overflowConfig.summarizeLastXTokens}
          onChange={v => setOverflow("summarizeLastXTokens", v)} min={1} />
        <NumField label="Max overflow tokens" value={summary.overflowConfig.maxOverflowSummaryTokens}
          onChange={v => setOverflow("maxOverflowSummaryTokens", v)} min={1} />
      </SubCard>

    </div>
  );
}

// ─── Main Component ───────────────────────────────────────────────────────────

export default function SettingsComponent() {
  const didMount = useRef(false);

  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  // Core settings state
  const [providers, setProviders] = useState<LLMProviderSettings[]>([]);
  const [presetMap, setPresetMap] = useState<Record<TagType, string>>({
    Main: "", Summarize: "", SummariesMerge: "", SceneTracker: "",
  });
  const [summarySettings, setSummarySettings] = useState<SummarySettings>(DEFAULT_SUMMARY);

  // Provider editing: null = none, "new" = adding, otherwise = provider id
  const [editingId, setEditingId] = useState<string | null>(null);
  const [pendingNew, setPendingNew] = useState<LLMProviderSettings | null>(null);

  // Presets
  const [availablePresets, setAvailablePresets] = useState<ChatCompletionPreset[]>([]);
  const [isLoadingPresets, setIsLoadingPresets] = useState(false);

  // Save state
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);

  useEffect(() => {
    if (didMount.current) return;
    didMount.current = true;
    fetchSettings();
    fetchPresets();
  }, []);

  const fetchSettings = async () => {
    try {
      setIsLoading(true);
      const response = await getFromServerApiAsync<SettingsResponseDto>("api/settings");
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code !== 200 || ex?.message) {
        console.error("Failed to fetch settings.", JSON.stringify(ex));
        setIsNetworkDown(true);
        return;
      }

      setProviders(response.llmProviders ?? []);

      if (response.summary)
        setSummarySettings(response.summary);

      const map: Record<TagType, string> = { Main: "", Summarize: "", SummariesMerge: "", SceneTracker: "" };
      const presetEntries = (response.chatCompletionPresetsMap?.map ?? []) as ChatCompletionPresetMapEntry[];
      for (const entry of presetEntries) {
        map[entry.type as TagType] = entry.chatCompletionPresetId;
      }
      setPresetMap(map);
    } catch (e) {
      console.error("Fetch settings error:", e);
      setIsNetworkDown(true);
    } finally {
      setIsLoading(false);
    }
  };

  const fetchPresets = async () => {
    try {
      setIsLoadingPresets(true);
      const response = await getFromServerApiAsync<ChatCompletionPresetsResponseDto>("api/chatCompletionPresets");
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code !== 200 || ex?.message) {
        console.error("Failed to fetch presets.", JSON.stringify(ex));
        return;
      }
      setAvailablePresets(response.chatCompletionPresets ?? []);
    } catch (e) {
      console.error("Fetch presets error:", e);
    } finally {
      setIsLoadingPresets(false);
    }
  };

  const handleApplyProvider = (updated: LLMProviderSettings) => {
    if (editingId === "new") {
      setProviders(prev => [...prev, updated]);
    } else {
      setProviders(prev => prev.map(p => p.providerConfigId === updated.providerConfigId ? updated : p));
    }
    setEditingId(null);
    setPendingNew(null);
  };

  const handleDeleteProvider = (id: string) => {
    setProviders(prev => prev.filter(p => p.providerConfigId !== id));
    setEditingId(null);
  };

  const handleAddProvider = () => {
    const blank = newProvider();
    setPendingNew(blank);
    setEditingId("new");
  };

  const handleCancelEdit = () => {
    setEditingId(null);
    setPendingNew(null);
  };

  const handleSave = async () => {
    if (isSaving) return;
    setIsSaving(true);
    setSaveError(false);
    setSaveSuccess(false);

    try {
      const response = await putToServerApiAsync("api/settings", {
        LLMProviders: providers,
        summary: summarySettings,
        chatCompletionPresetsMap: {
          map: ALL_TAGS
            .filter(tag => presetMap[tag])
            .map(tag => ({ type: tag, chatCompletionPresetId: presetMap[tag], isDefault: true })),
        },
      });

      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error("Save settings failed.", JSON.stringify(ex));
        setSaveError(true);
      } else {
        setSaveSuccess(true);
        setTimeout(() => setSaveSuccess(false), 2500);
      }
    } catch (e) {
      console.error("Save settings error:", e);
      setSaveError(true);
    } finally {
      setIsSaving(false);
    }
  };

  // ── Render ─────────────────────────────────────────────────────────────────

  if (isNetworkDown) {
    return (
      <main className={styles.settingsComponent}>
        <div className={styles.centered}>
          <AiOutlineDisconnect className={styles.networkIcon} />
          <span>CohesiveRP backend is unreachable</span>
        </div>
      </main>
    );
  }

  if (isLoading) {
    return (
      <main className={styles.settingsComponent}>
        <div className={styles.centered}>
          <ImSpinner2 className={styles.pageSpinner} />
        </div>
      </main>
    );
  }

  return (
    <main className={styles.settingsComponent}>
      <div className={styles.scroll}>

        {/* ── LLM Providers ───────────────────────────────────────────────── */}
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>LLM Providers</h2>
            <button
              className={styles.btnOutline}
              type="button"
              disabled={editingId !== null}
              onClick={handleAddProvider}
            >
              <MdAdd /> Add Provider
            </button>
          </div>

          <div className={styles.providerList}>
            {/* New provider editor */}
            {editingId === "new" && pendingNew && (
              <div className={`${styles.card} ${styles.cardActive}`}>
                <div className={styles.cardHeader}>
                  <span className={styles.cardHeaderTitle}>New Provider</span>
                </div>
                <ProviderEditor
                  initial={pendingNew}
                  isNew={true}
                  onApply={handleApplyProvider}
                  onCancel={handleCancelEdit}
                />
              </div>
            )}

            {providers.length === 0 && editingId !== "new" && (
              <p className={styles.emptyState}>No providers configured yet.</p>
            )}

            {providers.map(provider => (
              <div
                key={provider.providerConfigId}
                className={`${styles.card} ${editingId === provider.providerConfigId ? styles.cardActive : ""}`}
              >
                {editingId === provider.providerConfigId ? (
                  <>
                    <div className={styles.cardHeader}>
                      <span className={styles.cardHeaderTitle}>{provider.name || "Editing Provider"}</span>
                    </div>
                    <ProviderEditor
                      initial={provider}
                      isNew={false}
                      onApply={handleApplyProvider}
                      onCancel={handleCancelEdit}
                      onDelete={() => handleDeleteProvider(provider.providerConfigId)}
                    />
                  </>
                ) : (
                  <ProviderCard
                    provider={provider}
                    editingDisabled={editingId !== null}
                    onEdit={() => setEditingId(provider.providerConfigId)}
                  />
                )}
              </div>
            ))}
          </div>
        </section>

        {/* ── Completion Presets Map ───────────────────────────────────────── */}
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Completion Presets</h2>
          </div>
          <p className={styles.sectionSubtitle}>
            Assign a completion preset to each processing tag.
          </p>
          <div className={styles.presetList}>
            {ALL_TAGS.map(tag => (
              <PresetRow
                key={tag}
                tag={tag}
                selectedId={presetMap[tag]}
                presets={availablePresets}
                isLoading={isLoadingPresets}
                onChange={id => setPresetMap(prev => ({ ...prev, [tag]: id }))}
              />
            ))}
          </div>
        </section>

        {/* ── Summary ──────────────────────────────────────────────────────── */}
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Summary</h2>
          </div>
          <p className={styles.sectionSubtitle}>
            Token budgets and chunking behaviour for each summary tier.
          </p>
          <SummarySection summary={summarySettings} onChange={setSummarySettings} />
        </section>

        {/* ── Save Bar ─────────────────────────────────────────────────────── */}
        <div className={styles.saveBar}>
          <span className={styles.saveBarMessages}>
            {saveError && <span className={styles.msgError}>Failed to save. Please try again.</span>}
            {saveSuccess && <span className={styles.msgSuccess}>Settings saved.</span>}
          </span>
          <button
            className={styles.btnSave}
            type="button"
            disabled={isSaving || editingId !== null}
            onClick={handleSave}
          >
            {isSaving ? <ImSpinner2 className={styles.spinnerInline} /> : "Save Settings"}
          </button>
        </div>

      </div>
    </main>
  );
}
