import styles from "./IllustrationComponent.module.css";
import { useState, useEffect } from "react";
import { ImSpinner2 } from "react-icons/im";
import { deleteFromServerApiAsync, postToServerApiAsync } from "../../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { CharacterResponseDto } from "../../../../../../ResponsesDto/characters/CharacterResponseDto";
import type { CharacterMainAvatarIllustrationQueryRequestDto } from "../../../../../../RequestDto/characters/CharacterMainAvatarIllustrationQueryRequestDto";
import type { GeneratePromptInjectionForMainCharacterAvatarResponseDto } from "../../../../../../ResponsesDto/characters/GeneratePromptInjectionForMainCharacterAvatarResponseDto";
import { GetAvatarPathFromAvatarFilePath } from "../../../../../../utils/avatarUtils";
import { RiRobot2Fill } from "react-icons/ri";
import { FaImage } from "react-icons/fa";
import { MdChevronLeft, MdChevronRight, MdDelete } from "react-icons/md";
import ExpressionAvatarsComponent from "./ExpressionAvatarsComponent";
import type { ExpressionAvatarEntryDto, ExpressionAvatarsDto } from "./ExpressionAvatarsComponent";

/* ── Types ── */

type OutfitKey = "Clothed" | "Underwear" | "Naked";
const OUTFIT_OPTIONS: OutfitKey[] = ["Clothed", "Underwear", "Naked"];
const OUTFIT_ENUM: Record<OutfitKey, number> = {
  Clothed: 3,
  Underwear: 2,
  Naked: 1,
};

export type IllustrationOutfitEntry = {
  outfit: OutfitKey;
  illustratorPromptInjection: string;
};

interface Props {
  characterId: string | null;
  characterResponse: CharacterResponseDto | null;
  setCharacterResponse: React.Dispatch<React.SetStateAction<CharacterResponseDto | null>>;
  illustratorTag: string;
  setIllustratorTag: (v: string) => void;
  illustrationMapOutfits: IllustrationOutfitEntry[];
  setIllustrationMapOutfits: React.Dispatch<React.SetStateAction<IllustrationOutfitEntry[]>>;
  setOperationError: (v: boolean) => void;
}

export default function IllustrationComponent({
  characterId,
  characterResponse,
  setCharacterResponse,
  illustratorTag,
  setIllustratorTag,
  illustrationMapOutfits,
  setIllustrationMapOutfits,
  setOperationError,
}: Props) {
  const [isGeneratingAvatar, setIsGeneratingAvatar] = useState(false);
  const [isGeneratingPromptInjection, setIsGeneratingPromptInjection] = useState(false);
  const [isDeletingAvatar, setIsDeletingAvatar] = useState(false);
  const [selectedOutfit, setSelectedOutfit] = useState<OutfitKey>("Clothed");
  const [lightboxAvatar, setLightboxAvatar] = useState<{
    filePath: string;
    fileName: string;
    seed?: string | null;
  } | null>(null);

  /* ── Derive expressionAvatars per outfit from characterResponse ── */
  const expressionAvatarsByOutfit: ExpressionAvatarsDto[] =
    (characterResponse?.character?.imageGenerationConfiguration?.illustrationMapOutfits ?? []).map(
      (entry) => ({
        outfit: entry.outfit ?? "",
        // expressionAvatars is an array of ExpressionAvatarEntryDto
        expressionAvatars: (entry as any).expressionAvatars ?? [],
      })
    );

  /* ── Handler: avatar deleted from ExpressionAvatarsComponent ── */
  const handleExpressionAvatarDeleted = (outfit: string, expression: string, fileName: string) => {
    setCharacterResponse((prev) => {
      if (!prev?.character?.imageGenerationConfiguration?.illustrationMapOutfits) return prev;
      return {
        ...prev,
        character: {
          ...prev.character,
          imageGenerationConfiguration: {
            ...prev.character.imageGenerationConfiguration,
            illustrationMapOutfits:
              prev.character.imageGenerationConfiguration.illustrationMapOutfits.map((o) => {
                if ((o.outfit ?? "").toLowerCase() !== outfit.toLowerCase()) return o;
                return {
                  ...o,
                  expressionAvatars: ((o as any).expressionAvatars as ExpressionAvatarEntryDto[])
                    .map((entry) => {
                      if (entry.expression !== expression) return entry;
                      return {
                        ...entry,
                        avatars: entry.avatars.filter((a) => a.avatarFileName !== fileName),
                      };
                    }),
                };
              }),
          },
        },
      };
    });
  };

  /* ── Derived helpers (source avatars / main lightbox) ── */

  const getLightboxAvatars = () =>
    characterResponse?.character?.imageGenerationConfiguration?.illustrationMapOutfits
      ?.find((e) => (e.outfit ?? "").toLowerCase() === selectedOutfit.toLowerCase())
      ?.sourceAvatars ?? [];

  const getLightboxIndex = () =>
    getLightboxAvatars().findIndex((a) => a.avatarFileName === lightboxAvatar?.fileName);

  const handleLightboxNav = (direction: -1 | 1) => {
    const avatars = getLightboxAvatars();
    if (avatars.length < 2) return;
    const next = avatars[(getLightboxIndex() + direction + avatars.length) % avatars.length];
    setLightboxAvatar({
      filePath: next.avatarFilePath ?? "",
      fileName: next.avatarFileName ?? "",
      seed: next.avatarSeed,
    });
  };

  const getOutfitEntry = (outfit: OutfitKey) =>
    illustrationMapOutfits.find((e) => e.outfit === outfit) ?? {
      outfit,
      illustratorPromptInjection: "",
    };

  const setOutfitPrompts = (outfit: OutfitKey, value: string) =>
    setIllustrationMapOutfits((prev) => {
      const exists = prev.find((e) => e.outfit === outfit);
      if (exists)
        return prev.map((e) =>
          e.outfit === outfit ? { ...e, illustratorPromptInjection: value } : e
        );
      return [...prev, { outfit, illustratorPromptInjection: value }];
    });

  /* ── Keyboard navigation for main lightbox ── */
  useEffect(() => {
    if (!lightboxAvatar) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "ArrowLeft")  handleLightboxNav(-1);
      if (e.key === "ArrowRight") handleLightboxNav(1);
      if (e.key === "Escape")     setLightboxAvatar(null);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [lightboxAvatar, characterResponse, selectedOutfit]);

  /* ── Handlers ── */

  const handleGenerateAvatars = async () => {
    if (isGeneratingAvatar) return;
    setIsGeneratingAvatar(true);
    try {
      const payload: CharacterMainAvatarIllustrationQueryRequestDto = {
        characterId: characterId ?? null,
        personaId: null,
        outfit: OUTFIT_ENUM[selectedOutfit],
        type: 0,
      };
      const response = await postToServerApiAsync("api/illustrator/queries", payload);
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error(`Failed to generate avatar. [${JSON.stringify(ex)}]`);
        setOperationError(true);
      } else {
        console.log("Avatar generation query submitted successfully.");
      }
    } catch (error) {
      console.error("Generate avatar error:", error);
      setOperationError(true);
    } finally {
      setIsGeneratingAvatar(false);
    }
  };

  const handleGeneratePromptInjection = async () => {
    if (isGeneratingPromptInjection) return;
    setIsGeneratingPromptInjection(true);
    try {
      const payload = {
        characterId: characterId ?? null,
        outfit: OUTFIT_ENUM[selectedOutfit],
        type: 0,
      };
      const response = await postToServerApiAsync("api/illustrator/promptInjection", payload);
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error(`Failed to generate prompt injection. [${JSON.stringify(ex)}]`);
        setOperationError(true);
      } else {
        const typedResponse = response as GeneratePromptInjectionForMainCharacterAvatarResponseDto;
        for (const item of typedResponse.promptInjections ?? []) {
          const outfitKey = item.outfit as unknown as OutfitKey;
          if (outfitKey) setOutfitPrompts(outfitKey, item.content ?? "");
        }
      }
    } catch (error) {
      console.error("Generate prompt injection error:", error);
      setOperationError(true);
    } finally {
      setIsGeneratingPromptInjection(false);
    }
  };

  const handleDeleteAvatar = async () => {
    if (!lightboxAvatar?.fileName || !characterId || isDeletingAvatar) return;
    setIsDeletingAvatar(true);
    try {
      const response = await deleteFromServerApiAsync(
        `api/characters/${characterId}/avatars/${lightboxAvatar.fileName}`
      );
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error(`Failed to delete avatar [${lightboxAvatar.fileName}].`);
        return;
      }

      const currentAvatars = getLightboxAvatars();
      const currentIndex = currentAvatars.findIndex(
        (a) => a.avatarFileName === lightboxAvatar.fileName
      );
      const remainingAvatars = currentAvatars.filter(
        (a) => a.avatarFileName !== lightboxAvatar.fileName
      );
      const nextAvatarEntry =
        remainingAvatars.length > 0
          ? remainingAvatars[Math.min(currentIndex, remainingAvatars.length - 1)]
          : null;
      const nextLightbox = nextAvatarEntry
        ? {
            filePath: nextAvatarEntry.avatarFilePath ?? "",
            fileName: nextAvatarEntry.avatarFileName ?? "",
            seed: nextAvatarEntry.avatarSeed,
          }
        : null;

      setCharacterResponse((prev) => {
        if (!prev?.character?.imageGenerationConfiguration?.illustrationMapOutfits) return prev;
        return {
          ...prev,
          character: {
            ...prev.character,
            imageGenerationConfiguration: {
              ...prev.character.imageGenerationConfiguration,
              illustrationMapOutfits:
                prev.character.imageGenerationConfiguration.illustrationMapOutfits.map((o) => ({
                  ...o,
                  sourceAvatars: (o.sourceAvatars ?? []).filter(
                    (a) => a.avatarFileName !== lightboxAvatar.fileName
                  ),
                })),
            },
          },
        };
      });

      setLightboxAvatar(nextLightbox);
    } catch (err) {
      console.error("Delete avatar error:", err);
    } finally {
      setIsDeletingAvatar(false);
    }
  };

  /* ── Lightbox derived values ── */
  const lightboxAvatars = getLightboxAvatars();
  const lightboxIndex   = getLightboxIndex();
  const hasMultiple     = lightboxAvatars.length > 1;

  /* ── Current outfit source avatars ── */
  const currentAvatars =
    characterResponse?.character?.imageGenerationConfiguration?.illustrationMapOutfits
      ?.find((e) => (e.outfit ?? "").toLowerCase() === selectedOutfit.toLowerCase())
      ?.sourceAvatars ?? [];

  /* ── Render ── */
  return (
    <div className={styles.illustrationWrapper}>

      {/* ── Illustrator Tag ── */}
      <div className={styles.fieldGroup}>
        <label className={styles.fieldLabel}>Illustrator Tag</label>
        <textarea
          className={styles.fieldTextarea}
          style={{ minHeight: "1.3em", maxHeight: "3.2em" }}
          value={illustratorTag}
          onChange={(e) => setIllustratorTag(e.target.value)}
        />
      </div>

      {/* ── Outfit selector ── */}
      <div className={styles.fieldGroup}>
        <label className={styles.fieldLabel}>Outfit</label>
        <select
          className={styles.outfitSelect}
          value={selectedOutfit}
          onChange={(e) => setSelectedOutfit(e.target.value as OutfitKey)}
        >
          {OUTFIT_OPTIONS.map((o) => (
            <option key={o} value={o}>
              {o.charAt(0).toUpperCase() + o.slice(1)}
            </option>
          ))}
        </select>
      </div>

      {/* ── Prompt Injection ── */}
      <div className={styles.fieldGroup}>
        <div className={styles.labelRow}>
          <label className={styles.fieldLabel}>Illustrator Prompt Injection</label>
          <button
            className={styles.aiButton}
            onClick={handleGeneratePromptInjection}
            disabled={isGeneratingPromptInjection}
            title="Auto-generate prompt injection"
          >
            {isGeneratingPromptInjection ? (
              <ImSpinner2 className={styles.spinner} />
            ) : (
              <RiRobot2Fill />
            )}
          </button>
        </div>
        <textarea
          className={styles.fieldTextarea}
          value={getOutfitEntry(selectedOutfit).illustratorPromptInjection}
          onChange={(e) => setOutfitPrompts(selectedOutfit, e.target.value)}
        />
      </div>

      {/* ── Generate source Avatar ── */}
      <div className={styles.generateRow}>
        <button
          className={styles.generateAvatarButton}
          onClick={handleGenerateAvatars}
          disabled={isGeneratingAvatar}
          title="Queue source avatar generation"
        >
          {isGeneratingAvatar ? (
            <ImSpinner2 className={styles.spinner} />
          ) : (
            <>
              <FaImage />
              <span>Generate Avatars</span>
            </>
          )}
        </button>
      </div>

      {/* ── Source avatar carousel ── */}
      {currentAvatars.length > 0 && (
        <div className={styles.carouselContainer}>
          {currentAvatars.map((ap, i) => (
            <div
              key={i}
              className={styles.carouselItem}
              onClick={() =>
                setLightboxAvatar({
                  filePath: ap.avatarFilePath ?? "",
                  fileName: ap.avatarFileName ?? "",
                  seed: ap.avatarSeed,
                })
              }
            >
              <img
                src={GetAvatarPathFromAvatarFilePath(ap.avatarFilePath ?? "")}
                alt={ap.avatarFileName ?? `Avatar ${i + 1}`}
                onError={(e) => { e.currentTarget.style.opacity = "0.2"; }}
              />
              {ap.avatarSeed && (
                <span className={styles.carouselSeed}>{ap.avatarSeed}</span>
              )}
            </div>
          ))}
        </div>
      )}

      {/* ── Expression avatars sub-component ── */}
      <ExpressionAvatarsComponent
        characterId={characterId}
        selectedOutfit={selectedOutfit}
        expressionAvatarsByOutfit={expressionAvatarsByOutfit}
        onAvatarDeleted={handleExpressionAvatarDeleted}
        setOperationError={setOperationError}
      />

      {/* ── Main source-avatar lightbox ── */}
      {lightboxAvatar && (
        <div className={styles.lightboxOverlay} onClick={() => setLightboxAvatar(null)}>
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
              src={GetAvatarPathFromAvatarFilePath(lightboxAvatar.filePath)}
              alt={lightboxAvatar.fileName}
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
                {lightboxIndex + 1} / {lightboxAvatars.length}
              </span>
            )}

            <div className={styles.lightboxActions}>
              <button
                className={`${styles.lightboxBtn} ${styles.lightboxDeleteBtn}`}
                onClick={handleDeleteAvatar}
                disabled={isDeletingAvatar}
                title="Delete avatar"
              >
                {isDeletingAvatar ? (
                  <ImSpinner2 className={styles.lightboxSpinner} />
                ) : (
                  <MdDelete style={{ fill: "currentColor" }} />
                )}
              </button>
              <button
                className={`${styles.lightboxBtn} ${styles.lightboxCloseBtn}`}
                onClick={() => setLightboxAvatar(null)}
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
