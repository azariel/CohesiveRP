import { useEffect, useRef, useState } from "react";
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
  if (value >= 20) return styles["tier-critical"];
  if (value >= 14) return styles["tier-high"];
  if (value >= 10) return styles["tier-mid"];
  if (value >= 1) return styles["tier-low"];
  return styles["tier-critical-fail"];
}

// Counter-roll color relative to the main roll's total (value + bonus)
function relativeTierClass(counterValue: number, mainTotal: number): string {
  const diff = mainTotal - counterValue;
  if (diff < 0) return styles["counter-tier-higher"];      // counter beats main
  if (diff === 0) return styles["counter-tier-equal"];      // tied
  if (diff <= 3) return styles["counter-tier-close"];       // lower by ≤3
  if (diff <= 7) return styles["counter-tier-behind"];      // lower by ≤7
  return styles["counter-tier-far-behind"];                 // lower by >7
}

// ── Sub-components ────────────────────────────────────────────────────────────

function RollRow({ roll }: { roll: ChatCharacterRoll }) {
  const [showGuides, setShowGuides] = useState(false);
  const hasGuides = roll.guides && roll.guides.length > 0;
  const hasCounters   = roll.charactersInSceneWithCounterRolls?.length > 0;
  const mainTotal = roll.value + roll.bonus;

  return (
    <div className={styles.rollRow}>
      {/* Skill pill + roll value */}
      <div
        className={`${styles.actionPill} ${tierClass(roll.value)}`}
        title={`${roll.actionCategory}: ${roll.value}`}
      >
        <span className={styles.skillLabel}>{roll.actionCategory}</span>
        <span className={styles.rollValue}>{roll.value}</span>
        {roll.bonus !== 0 && <span className={styles.rollValue}>( {(roll.bonus > 0 ? `+ ${roll.bonus}` : roll.bonus)} )</span>}
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
                className={`${styles.counterPill} ${relativeTierClass(value, mainTotal)}`}
                title={`${cr.characterName} — ${String(attr)}: ${value}`}
              >
                <span className={styles.counterCharName}>{cr.characterName}</span>
                <span className={styles.counterAttrLabel}>({attrShort})</span><span />
                <span className={styles.counterValue}>{value}</span>
              </div>
            );
          })}
        </div>
      )}

      {/* Guides/Reasonings toggle */}
      {hasGuides && (
        <button
          className={styles.reasoningBtn}
          onClick={() => setShowGuides(p => !p)}
          title="Show reasoning"
        >
          {showGuides ? "▲ why" : "▼ why"}
        </button>
      )}

      {/* Reasoning list (expanded) */}
      {/* Guide list (expanded) */}
      {showGuides && hasGuides && (
        <div className={styles.reasoningList}>
          {roll.guides.map((r, i) => (
            <div key={i} className={styles.reasoningItem}>
              <span>
                <p>{r.reasoning}</p>
                <p className={styles.reasoningItemSuccess}>{r.reactionFromOtherCharactersWhenSucceedingSkillCheck}</p>
                <p className={styles.reasoningItemFailure}>{r.reactionFromOtherCharactersWhenFailingSkillCheck}</p>
              </span>
            </div>
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
  const { activeModule, setActiveModule } = sharedContext<SharedContextChatType>();
  const didFetchOnce = useRef(false);
  const isOpen = activeModule?.isCharactersRollsOpened ?? false;
  const [isLoading, setIsLoading] = useState(false);
  const [rolls, setRolls]         = useState<ChatCharacterRollResponse[]>([]);

  useEffect(() => {
    if (!activeModule?.chatId) return;
    if (didFetchOnce.current && sceneTrackerRefreshToken === undefined) return;

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

      setActiveModule((prev) =>
        prev ? { ...prev, latestPlayerDescription: response.playerDescription } : prev
      );
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
        <button className={styles.header} onClick={() =>
            setActiveModule((prev) =>
              prev ? { ...prev, isCharactersRollsOpened: !isOpen } : prev
            )
          }>
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
