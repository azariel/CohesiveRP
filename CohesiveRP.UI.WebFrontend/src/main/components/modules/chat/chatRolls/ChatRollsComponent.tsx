import { useEffect, useState } from "react";
import { HiMiniChevronRight } from "react-icons/hi2";
import { ImSpinner2 } from "react-icons/im";
import { GiDiceTwentyFacesTwenty } from "react-icons/gi";

import styles from "./ChatRollsComponent.module.css";
import { getFromServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import { sharedContext } from "../../../../../store/AppSharedStoreContext";
import type { SharedContextChatType } from "../../../../../store/SharedContextChatType";
import type { ChatCharacterRoll, ChatCharacterRollResponse, ChatCharacterRollsResponseDto } from "../../../../../ResponsesDto/ChatCharacterRollsResponseDto";

// ── Colour tier ───────────────────────────────────────────────────────────────

function tierClass(value: number): string {
  if (value >= 18) return styles["tier-critical"];
  if (value >= 14) return styles["tier-high"];
  if (value >= 9)  return styles["tier-mid"];
  if (value >= 5)  return styles["tier-low"];
  return styles["tier-critical-fail"];
}

// ── Sub-components ────────────────────────────────────────────────────────────

function RollRow({ roll }: { roll: ChatCharacterRoll }) {
  const [showReasonings, setShowReasonings] = useState(false);
  const hasReasonings = roll.reasonings && roll.reasonings.length > 0;
  const hasCounters   = roll.charactersInSceneWithCounterRolls?.length > 0;

  return (
    <div className={styles.rollRow}>
      {/* Skill pill + roll value */}
      <div
        className={`${styles.actionPill} ${tierClass(roll.value)}`}
        title={`${roll.actionCategory}: ${roll.value}`}
      >
        <span className={styles.skillLabel}>{roll.actionCategory}</span>
        <span className={styles.rollValue}>{roll.value}</span>
      </div>

      {/* Counter rolls from other characters in the scene */}
      {hasCounters && (
        <div className={styles.counterRolls}>
          <span className={styles.vsLabel}>vs</span>
          {roll.charactersInSceneWithCounterRolls.map((cr, idx) => {
            const attr  = cr.characterInSceneCounterRoll.attribute ?? "Unknown";
            const value = cr.characterInSceneCounterRoll.value;
            const attrShort = attr.length > 3 ? attr.slice(0, 3) : attr;
            return (
              <div
                key={`${cr.characterId}-${idx}`}
                className={`${styles.counterPill} ${tierClass(value)}`}
                title={`${cr.characterName} — ${String(attr)}: ${value}`}
              >
                <span className={styles.counterCharName}>{cr.characterName}</span>
                <span className={styles.counterAttrLabel}>{attrShort}</span>
                <span className={styles.counterValue}>{value}</span>
              </div>
            );
          })}
        </div>
      )}

      {/* Reasoning toggle */}
      {hasReasonings && (
        <button
          className={styles.reasoningBtn}
          onClick={() => setShowReasonings(p => !p)}
          title="Show reasoning"
        >
          {showReasonings ? "▲ why" : "▼ why"}
        </button>
      )}

      {/* Reasoning list (expanded) */}
      {showReasonings && hasReasonings && (
        <div className={styles.reasoningList}>
          {roll.reasonings.map((r, i) => (
            <span key={i} className={styles.reasoningItem}>{r}</span>
          ))}
        </div>
      )}
    </div>
  );
}

function CharacterBlock({ character }: { character: ChatCharacterRollResponse }) {
  return (
    <div className={styles.characterBlock}>
      <div className={styles.characterName}>{character.characterName}</div>
      {character.rolls.map((roll, idx) => (
        <RollRow key={`${character.characterId}-${idx}`} roll={roll} />
      ))}
    </div>
  );
}

// ── Main component ────────────────────────────────────────────────────────────

interface Props {
  /** Increment this (via sceneTrackerRefreshToken in SharedContext) to trigger a re-fetch after each generation */
  sceneTrackerRefreshToken?: number;
}

export default function ChatRollsComponent({ sceneTrackerRefreshToken }: Props) {
  const { activeModule } = sharedContext<SharedContextChatType>();
  const [isOpen,    setIsOpen]    = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [rolls, setRolls]         = useState<ChatCharacterRollResponse[]>([]);

  useEffect(() => {
    if (!activeModule?.chatId) return;

    const abort = new AbortController();

    const fetchRolls = async () => {
      setIsLoading(true);

      const response = await getFromServerApiAsync<ChatCharacterRollsResponseDto>(
        `api/chatCharacterRolls/chats/${activeModule.chatId}`,
        abort.signal
      );

      if (abort.signal.aborted) return;

      const err = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code !== 200 || err?.message) {
        console.error(`Fetching character rolls failed. Code:[${response?.code}]`);
        setIsLoading(false);
        return;
      }

      setRolls(response.rolls ?? []);
      setIsLoading(false);
    };

    fetchRolls();
    return () => abort.abort();

  }, [activeModule?.chatId, sceneTrackerRefreshToken]);

  // Don't occupy space until there's something to show
  if (!isLoading && rolls.length === 0) return null;

  const totalChecks = rolls.reduce((acc, c) => acc + (c.rolls?.length ?? 0), 0);

  return (
    <div className={styles.rollsWrapper}>
      <div className={styles.rollsContainer}>

        {/* Header / toggle */}
        <button className={styles.header} onClick={() => setIsOpen(p => !p)}>
          {isLoading
            ? <ImSpinner2 className={`${styles.diceIcon} ${styles.spinner}`} />
            : <GiDiceTwentyFacesTwenty className={styles.diceIcon} />
          }
          <span className={styles.headerLabel}>
            {isLoading
              ? "Rolling…"
              : `Scene Rolls · ${totalChecks} check${totalChecks !== 1 ? "s" : ""}`
            }
          </span>
          {!isLoading && (
            <HiMiniChevronRight
              className={`${styles.chevron} ${isOpen ? styles.chevronOpen : ""}`}
            />
          )}
        </button>

        {/* Body */}
        {isOpen && !isLoading && (
          <div className={styles.body}>
            {rolls.length === 0 ? (
              <span className={styles.emptyState}>No rolls for this scene.</span>
            ) : (
              rolls.map(character => (
                <CharacterBlock key={character.characterId} character={character} />
              ))
            )}
          </div>
        )}

      </div>
    </div>
  );
}
