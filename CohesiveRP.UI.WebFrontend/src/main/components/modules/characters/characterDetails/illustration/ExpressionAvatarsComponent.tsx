import styles from "./ExpressionAvatarsComponent.module.css";
import { useState, useEffect } from "react";
import { ImSpinner2 } from "react-icons/im";
import { FaImage } from "react-icons/fa";
import { MdChevronLeft, MdChevronRight, MdDelete } from "react-icons/md";
import { deleteFromServerApiAsync, postToServerApiAsync } from "../../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import { GetAvatarPathFromAvatarFilePath } from "../../../../../../utils/avatarUtils";

/* ── Expression (const object replaces enum — erasableSyntaxOnly safe) ── */

export const Expression = {
  Admiration:     "Admiration",
  Amusement:      "Amusement",
  Anger:          "Anger",
  Annoyance:      "Annoyance",
  Arousal:        "Arousal",
  Arrogant:       "Arrogant",
  Bored:          "Bored",
  Confusion:      "Confusion",
  Crying:         "Crying",
  Curiosity:      "Curiosity",
  Disappointment: "Disappointment",
  Disapproval:    "Disapproval",
  Disgust:        "Disgust",
  Embarrassment:  "Embarrassment",
  Excitement:     "Excitement",
  Fear:           "Fear",
  Gratitude:      "Gratitude",
  Grief:          "Grief",
  Jealousy:       "Jealousy",
  Joy:            "Joy",
  Laughing:       "Laughing",
  Nervousness:    "Nervousness",
  Neutral:        "Neutral",
  Pride:          "Pride",
  Realization:    "Realization",
  Relief:         "Relief",
  Remorse:        "Remorse",
  Sadness:        "Sadness",
  Serious:        "Serious",
  Shy:            "Shy",
  Sleepy:         "Sleepy",
  Surprised:      "Surprised",
  Worried:        "Worried",
} as const;

export type Expression = (typeof Expression)[keyof typeof Expression];

const EXPRESSION_OPTIONS = Object.values(Expression);

/* ── Types matching the new C# response shape ── */

export type CharacterAvatarDto = {
  avatarFilePath: string;
  avatarFileName: string;
  avatarSeed?: string | null;
};

// The shape as it actually arrives from the backend
export type ExpressionAvatarEntryDto = {
  expression: string;
  avatars: CharacterAvatarDto[];
};

export type ExpressionAvatarsDto = {
  outfit: string;
  expressionAvatars: ExpressionAvatarEntryDto[]; // ← array, not Record
};

type OutfitKey = "Clothed" | "Underwear" | "Naked";
const OUTFIT_ENUM: Record<OutfitKey, number> = { Clothed: 3, Underwear: 2, Naked: 1 };

type LightboxState = {
  filePath: string;
  fileName: string;
  seed?: string | null;
};

interface Props {
  characterId: string | null;
  selectedOutfit: OutfitKey;
  /** expressionAvatars data lifted from characterResponse per outfit */
  expressionAvatarsByOutfit: ExpressionAvatarsDto[];
  onAvatarDeleted: (outfit: string, expression: string, fileName: string) => void;
  setOperationError: (v: boolean) => void;
}

export default function ExpressionAvatarsComponent({
  characterId,
  selectedOutfit,
  expressionAvatarsByOutfit,
  onAvatarDeleted,
  setOperationError,
}: Props) {
  const [selectedExpression, setSelectedExpression] = useState<Expression>(Expression.Neutral);
  const [isGenerating, setIsGenerating] = useState(false);
  const [isDeletingAvatar, setIsDeletingAvatar] = useState(false);
  const [lightbox, setLightbox] = useState<LightboxState | null>(null);

  /* ── Derive current avatar list ── */

const currentAvatars: CharacterAvatarDto[] =
  expressionAvatarsByOutfit
    .find((e) => e.outfit.toLowerCase() === selectedOutfit.toLowerCase())
    ?.expressionAvatars
    .find((e) => e.expression === selectedExpression)
    ?.avatars ?? [];

  /* ── Lightbox helpers ── */

  const getLightboxIndex = () =>
    currentAvatars.findIndex((a) => a.avatarFileName === lightbox?.fileName);

  const handleLightboxNav = (direction: -1 | 1) => {
    if (currentAvatars.length < 2) return;
    const next = currentAvatars[(getLightboxIndex() + direction + currentAvatars.length) % currentAvatars.length];
    setLightbox({ filePath: next.avatarFilePath, fileName: next.avatarFileName, seed: next.avatarSeed });
  };

  /* ── Keyboard navigation ── */
  useEffect(() => {
    if (!lightbox) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "ArrowLeft")  handleLightboxNav(-1);
      if (e.key === "ArrowRight") handleLightboxNav(1);
      if (e.key === "Escape")     setLightbox(null);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [lightbox, currentAvatars]);

  /* ── Generate ── */
  const handleGenerate = async () => {
    if (isGenerating || !characterId) return;
    setIsGenerating(true);
    try {
      const payload = {
        characterId,
        personaId: null,
        type: 1,
        outfit: OUTFIT_ENUM[selectedOutfit],
        expressions: [selectedExpression],
      };
      const response = await postToServerApiAsync("api/illustrator/queries", payload);
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error(`Failed to queue expression avatar generation. [${JSON.stringify(ex)}]`);
        setOperationError(true);
      } else {
        console.log(`Expression avatar generation queued: ${selectedOutfit}/${selectedExpression}`);
      }
    } catch (err) {
      console.error("Generate expression avatar error:", err);
      setOperationError(true);
    } finally {
      setIsGenerating(false);
    }
  };

  /* ── Generate Missing ── */
  const handleGenerateMissing = async () => {
    if (isGenerating || !characterId) return;
    setIsGenerating(true);
    try {
      const currentOutfitEntry = expressionAvatarsByOutfit
        .find((e) => e.outfit.toLowerCase() === selectedOutfit.toLowerCase());

      const missingExpressions = EXPRESSION_OPTIONS.filter((expr) => {
        const avatars = currentOutfitEntry?.expressionAvatars
          .find((e) => e.expression === expr)?.avatars ?? [];
        return avatars.length === 0;
      });

      if (missingExpressions.length === 0) return;

      const payload = {
        characterId,
        personaId: null,
        type: 1,
        outfit: OUTFIT_ENUM[selectedOutfit],
        expressions: missingExpressions,
      };
      const response = await postToServerApiAsync("api/illustrator/queries", payload);
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error(`Failed to queue missing expression avatars. [${JSON.stringify(ex)}]`);
        setOperationError(true);
      } else {
        console.log(`Missing expression avatars queued (${missingExpressions.length}): ${missingExpressions.join(", ")}`);
      }
    } catch (err) {
      console.error("Generate missing expression avatars error:", err);
      setOperationError(true);
    } finally {
      setIsGenerating(false);
    }
  };

  /* ── Delete ── */
  const handleDeleteAvatar = async () => {
    if (!lightbox?.fileName || !characterId || isDeletingAvatar) return;
    setIsDeletingAvatar(true);
    try {
      const response = await deleteFromServerApiAsync(
        `api/characters/${characterId}/avatars/${lightbox.fileName}`
      );
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error(`Failed to delete expression avatar [${lightbox.fileName}].`);
        return;
      }

      // Determine next lightbox target before mutating
      const remaining = currentAvatars.filter((a) => a.avatarFileName !== lightbox.fileName);
      const idx = currentAvatars.findIndex((a) => a.avatarFileName === lightbox.fileName);
      const nextEntry = remaining.length > 0
        ? remaining[Math.min(idx, remaining.length - 1)]
        : null;

      onAvatarDeleted(selectedOutfit, selectedExpression, lightbox.fileName);
      setLightbox(
        nextEntry
          ? { filePath: nextEntry.avatarFilePath, fileName: nextEntry.avatarFileName, seed: nextEntry.avatarSeed }
          : null
      );
    } catch (err) {
      console.error("Delete expression avatar error:", err);
    } finally {
      setIsDeletingAvatar(false);
    }
  };

  const lightboxIndex = getLightboxIndex();
  const hasMultiple   = currentAvatars.length > 1;

  /* ── Render ── */
  return (
    <div className={styles.expressionWrapper}>

      {/* ── Header row: label + expression selector ── */}
      <div className={styles.headerRow}>
        <span className={styles.sectionLabel}>Expression Avatars</span>
        <button
          className={styles.generateMissingButton}
          onClick={handleGenerateMissing}
          disabled={isGenerating || !characterId}
          title="Queue generation for all expressions with no avatars"
        >
          {isGenerating ? (
            <ImSpinner2 className={styles.spinner} />
          ) : (
            <>
              <FaImage />
              <span>Generate Missing</span>
            </>
          )}
        </button>
        <select
          className={styles.expressionSelect}
          value={selectedExpression}
          onChange={(e) => setSelectedExpression(e.target.value as Expression)}
        >
          {EXPRESSION_OPTIONS.map((ex) => (
            <option key={ex} value={ex}>{ex}</option>
          ))}
        </select>
      </div>

      {/* ── Generate button ── */}
      <div className={styles.generateRow}>
        <button
          className={styles.generateButton}
          onClick={handleGenerate}
          disabled={isGenerating || !characterId}
          title={`Queue generation for ${selectedExpression} / ${selectedOutfit}`}
        >
          {isGenerating ? (
            <ImSpinner2 className={styles.spinner} />
          ) : (
            <>
              <FaImage />
              <span>Generate {selectedExpression}</span>
            </>
          )}
        </button>
      </div>

      {/* ── Carousel ── */}
      {currentAvatars.length > 0 ? (
        <div className={styles.carousel}>
          {currentAvatars.map((av, i) => (
            <div
              key={i}
              className={styles.carouselItem}
              onClick={() => setLightbox({ filePath: av.avatarFilePath, fileName: av.avatarFileName, seed: av.avatarSeed })}
            >
              <img
                src={GetAvatarPathFromAvatarFilePath(av.avatarFilePath)}
                alt={av.avatarFileName ?? `${selectedExpression} ${i + 1}`}
                onError={(e) => { e.currentTarget.style.opacity = "0.2"; }}
              />
              {av.avatarSeed && (
                <span className={styles.carouselSeed}>{av.avatarSeed}</span>
              )}
            </div>
          ))}
        </div>
      ) : (
        <p className={styles.emptyLabel}>No avatars for {selectedExpression} · {selectedOutfit}</p>
      )}

      {/* ── Lightbox ── */}
      {lightbox && (
        <div className={styles.lightboxOverlay} onClick={() => setLightbox(null)}>
          <div className={styles.lightboxCard} onClick={(e) => e.stopPropagation()}>

            {hasMultiple && (
              <button
                className={`${styles.lightboxNavBtn} ${styles.lightboxNavPrev}`}
                onClick={() => handleLightboxNav(-1)}
                title="Previous (←)"
              >
                <MdChevronLeft />
              </button>
            )}

            <img
              className={styles.lightboxImage}
              src={GetAvatarPathFromAvatarFilePath(lightbox.filePath)}
              alt={lightbox.fileName}
              onError={(e) => { e.currentTarget.style.opacity = "0.2"; }}
            />

            {hasMultiple && (
              <button
                className={`${styles.lightboxNavBtn} ${styles.lightboxNavNext}`}
                onClick={() => handleLightboxNav(1)}
                title="Next (→)"
              >
                <MdChevronRight />
              </button>
            )}

            {hasMultiple && (
              <span className={styles.lightboxCounter}>
                {lightboxIndex + 1} / {currentAvatars.length}
              </span>
            )}

            <div className={styles.lightboxActions}>
              <button
                className={`${styles.lightboxBtn} ${styles.lightboxDeleteBtn}`}
                onClick={handleDeleteAvatar}
                disabled={isDeletingAvatar}
                title="Delete avatar"
              >
                {isDeletingAvatar
                  ? <ImSpinner2 className={styles.lightboxSpinner} />
                  : <MdDelete style={{ fill: "currentColor" }} />}
              </button>
              <button
                className={`${styles.lightboxBtn} ${styles.lightboxCloseBtn}`}
                onClick={() => setLightbox(null)}
                title="Close"
              >
                ✕
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
