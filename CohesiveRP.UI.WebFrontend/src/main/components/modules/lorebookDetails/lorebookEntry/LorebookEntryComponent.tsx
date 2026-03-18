import { useState } from "react";
import type { LorebookEntry } from "../../../../../ResponsesDto/lorebooks/BusinessObjects/LorebookEntry";
import styles from "./LorebookEntryComponent.module.css";
// import { useRef, useEffect, useState } from "react";

/* Store */
// import { sharedContext } from '../../../../../store/AppSharedStoreContext';
// import type { SharedContextLorebookType } from "../../../../../store/SharedContextLorebookType";
// import type { LorebookResponseDto } from "../../../../../ResponsesDto/lorebooks/LorebookResponseDto";

interface Props {
  entry: LorebookEntry;
  onEntryChange: (updated: LorebookEntry) => void;
}

export default function LorebookEntryComponent({ entry, onEntryChange  }:Props) {
  const [content, setContent]               = useState(entry.content ?? "");
  const [keys, setKeys]                     = useState((entry.keys ?? []).join(", "));
  const [enabled, setEnabled]               = useState(entry.enabled);
  const [insertionOrder, setInsertionOrder] = useState(entry.insertionOrder);
  const [useRegex, setUseRegex]             = useState(entry.useRegex);
  const [constant, setConstant]             = useState(entry.constant);

  const notify = (patch: Partial<LorebookEntry>) => {
    onEntryChange({
      content,
      keys: keys.split(",").map(k => k.trim()).filter(Boolean),
      enabled,
      insertionOrder,
      useRegex,
      constant,
      ...patch,
    });
  };

  return (
    <div className={styles.lorebookEntryComponent}>
      <textarea
        value={content}
        onChange={e => { setContent(e.target.value); notify({ content: e.target.value }); }}
      />
      <input
        value={keys}
        placeholder="keys, comma-separated"
        onChange={e => {
          setKeys(e.target.value);
          notify({ keys: e.target.value.split(",").map(k => k.trim()).filter(Boolean) });
        }}
      />
      <label>
        <input type="checkbox" checked={enabled}
          onChange={e => { setEnabled(e.target.checked); notify({ enabled: e.target.checked }); }} />
        Enabled
      </label>
      <label>
        <input type="checkbox" checked={constant}
          onChange={e => { setConstant(e.target.checked); notify({ constant: e.target.checked }); }} />
        Constant
      </label>
      <label>
        <input type="checkbox" checked={useRegex}
          onChange={e => { setUseRegex(e.target.checked); notify({ useRegex: e.target.checked }); }} />
        Use Regex
      </label>
      <input
        type="number"
        value={insertionOrder}
        onChange={e => { const v = Number(e.target.value); setInsertionOrder(v); notify({ insertionOrder: v }); }}
      />
    </div>
  );
}