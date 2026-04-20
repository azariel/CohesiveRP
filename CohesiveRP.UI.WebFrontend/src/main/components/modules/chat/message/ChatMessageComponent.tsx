import { useState, useRef, useEffect } from "react";
import type { ChatMessage } from "../../../../../ResponsesDto/chat/BusinessObjects/ChatMessage";
import styles from "./ChatMessageComponent.module.css";
import { HiAdjustmentsHorizontal, HiBeaker, HiMiniUsers, HiChatBubbleLeftEllipsis, HiCircleStack, HiCog6Tooth, HiIdentification, HiMiniChevronRight } from "react-icons/hi2";
import { GrRevert } from "react-icons/gr";
import { MdOutlineSummarize } from "react-icons/md";
import { FormatDateTimeDurationMinutesAndSeconds, FormatUtcDate, ParseFocusedGenerationDate } from "../../../../../utils/DateUtils";
import { HighlightedText } from "../../../../../utils/HighlightText";
import { getAvatarPathFromCharacterAvatarDefinition, GetAvatarPathFromChatIdAndAvatarId, GetAvatarPathFromPersonaId } from "../../../../../utils/avatarUtils";
import { FaTrashAlt } from "react-icons/fa";
import { getFromServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { PromptResponseDto } from "../../../../../ResponsesDto/chat/PromptResponseDto";

interface Props {
  message?: ChatMessage;
  chatId: string;
  enableDeleteBtn?: boolean;
  enableSwipeBtn?: boolean;
  isEditable?: boolean;
  onSave?: (messageId: string, newContent: string) => Promise<void>;
  onDelete?: (messageId: string) => Promise<void>;
}

export default function ChatMessageComponent({ message, chatId, enableSwipeBtn = false, enableDeleteBtn = false, isEditable = false, onSave, onDelete }: Props) {
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState(message?.content ?? "");
  const [isThinkingExpanded, setIsThinkingExpanded] = useState(false);
  const isRevertingRef = useRef(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const start = ParseFocusedGenerationDate(message?.startGenerationDateTimeUtc);
  const startFocused = ParseFocusedGenerationDate(message?.startFocusedGenerationDateTimeUtc);
  const end = ParseFocusedGenerationDate(message?.endFocusedGenerationDateTimeUtc);
  const durationMs = startFocused !== null && end !== null ? (end - startFocused) : null;
  const totalDurationMs = start !== null && end !== null ? (end - start) : null;

  // Prompt
  const [isPromptModalOpen, setIsPromptModalOpen] = useState(false);
  const [promptText, setPromptText] = useState<string | null>(null);
  const [isLoadingPrompt, setIsLoadingPrompt] = useState(false);
  const [promptCopied, setPromptCopied] = useState(false);

  // Focus + size once when entering edit mode
  useEffect(() => {
    if (!isEditing || !textareaRef.current)
      return;

    const el = textareaRef.current;
    el.style.height = "auto";
    el.style.height = `${el.scrollHeight}px`;
    el.focus();
    el.setSelectionRange(el.value.length, el.value.length);

    const isMobile = navigator.maxTouchPoints > 0;
    if (isMobile) {
      setTimeout(() => {
        el.scrollIntoView({ block: "end", behavior: "smooth" });
      }, 300);
    }
  }, [isEditing]);

  const handleDoubleClick = () => {
    if (!isEditable)
      return;

    setEditContent(message?.content ?? "");
    setIsEditing(true);
  };

  const handleRevert = () => {
    isRevertingRef.current = true;
    setEditContent(message?.content ?? "");
    setIsEditing(false);
    isRevertingRef.current = false;
  };

  const handleDelete = async () => {
    if (!message?.messageId || !onDelete)
      return;

    await onDelete(message.messageId);
  };

  const handleBlur = async () => {
    if (isRevertingRef.current) {
      isRevertingRef.current = false;
      return;
    }

    setIsEditing(false);
    const trimmed = editContent.trim();

    if (!trimmed || trimmed === message?.content || !message?.messageId || !onSave)
      return;

    await onSave(message.messageId, trimmed);
  };

  // Prompt
  const handleShowPrompt = async () => {
    if (!chatId) return;
    setIsPromptModalOpen(true);
    setIsLoadingPrompt(true);
    setPromptText(null);

    const response = await getFromServerApiAsync<PromptResponseDto>(`api/chat/${chatId}/prompt`);
    setIsLoadingPrompt(false);

    const err = response as ServerApiExceptionResponseDto | null;
    if (!response || response.code !== 200 || err?.message) {
      console.error(`Failed to fetch prompt. Code:[${response?.code}]`);
      setPromptText("[Error: could not load prompt.]");
      return;
    }

    setPromptText(response.prompt);
  };

  const handleCopyPrompt = async () => {
    if (!promptText) return;

    try {
      if (navigator.clipboard?.writeText) {
        await navigator.clipboard.writeText(promptText);
      } else {
        // Fallback for HTTP / older browsers
        const el = document.createElement("textarea");
        el.value = promptText;
        el.style.position = "fixed";
        el.style.opacity = "0";
        document.body.appendChild(el);
        el.focus();
        el.select();
        document.execCommand("copy");
        document.body.removeChild(el);
      }
      setPromptCopied(true);
      setTimeout(() => setPromptCopied(false), 2000);
    } catch (err) {
      console.error("Failed to copy prompt:", err);
    }
  };

  const displayedContent = message?.content ?? "[empty]";

  return (
    <main className={styles.chatMessageComponent}>
      <div className={styles.container}>
        <div className={styles.leftMessageContainer}>
          <div className={styles.messageAvatarContainer}>
            <div className={styles.messageAvatarContainer}>
              {(() => {
                const avatarPath = message?.characterAvatars?.[0];
                const src = message?.sourceType === 0
                  ? (avatarPath && avatarPath.expression !== null
                      ? getAvatarPathFromCharacterAvatarDefinition(avatarPath)
                      : GetAvatarPathFromPersonaId(message?.personaId ?? ""))
                  : (avatarPath && avatarPath.expression !== null
                      ? getAvatarPathFromCharacterAvatarDefinition(avatarPath)
                      : GetAvatarPathFromChatIdAndAvatarId(chatId, "avatar"));
                return <img src={src} alt="Avatar" onError={(e) => { e.currentTarget.src = GetAvatarPathFromChatIdAndAvatarId(chatId, "avatar"); }} />;
              })()}
            </div>
          </div>
          <div className={styles.messageInfoContainer}>
            <div title="messageId">{!message?.messageIndex ? "-" : "# " + message.messageIndex}</div>
          </div>
        </div>
        <div className={styles.messageContent}>
          <div className={styles.messageHeaderContent}>
            <div className={styles.messageHeaderContentName}>
              {message?.sourceType == 0 ? <label>{message?.personaName ?? "User"}</label> : <label>{message?.characterName ?? "Character"}</label>}
            </div>
            <div className={styles.messageHeaderContentModel}>
              <label>{FormatDateTimeDurationMinutesAndSeconds(totalDurationMs) ?? "-"} ({FormatDateTimeDurationMinutesAndSeconds(durationMs) ?? "-"})</label>
            </div>
            <div className={styles.messageHeaderContentCreatedAt}>
              {message?.summarized ? (<MdOutlineSummarize className={styles.messageHeaderSummarizeIcon} title="Summarized" />) : ""}
              {message?.createdAtUtc ? FormatUtcDate(message.createdAtUtc) : ""}
            </div>
          </div>
          <div className={styles.messageContentSeparator} />

          {message?.thinkingContent && (
            <div className={styles.thinkingContainer}>
              <button
                className={styles.thinkingToggle}
                onClick={() => setIsThinkingExpanded(prev => !prev)}
              >
                <HiBeaker className={styles.thinkingIcon} />
                <span>Thinking</span>
                <HiMiniChevronRight
                  className={`${styles.thinkingChevron} ${isThinkingExpanded ? styles.thinkingChevronOpen : ""}`}
                />
              </button>
              {isThinkingExpanded && (
                <div className={styles.thinkingContent}>
                  <HighlightedText text={message.thinkingContent} />
                </div>
              )}
            </div>
          )}

          {isEditing ? (
            <div>
              <textarea
                ref={textareaRef}
                className={styles.messageContentValueEditArea}
                value={editContent}
                onChange={(e) => setEditContent(e.target.value)}
                onBlur={handleBlur}
              />
              <GrRevert
                className={styles.messageRevertBtn}
                onMouseDown={(e) => e.preventDefault()}
                onClick={handleRevert}
              />
            </div>
          ) : (
            <div
              className={`${styles.messageContentValue} ${isEditable ? styles.messageContentValueEditable : ""}`}
              onDoubleClick={handleDoubleClick}
            >
              <HighlightedText text={displayedContent} />
            </div>
          )}

          <div className={styles.messageContentFooter}>
            <div className={styles.messageContentFooterLeftSideIcons}>
              <HiChatBubbleLeftEllipsis
                className={styles.footerIconBtn}
                onClick={handleShowPrompt}
                title="View prompt"
              />
              <HiMiniUsers />
              <HiIdentification />
              <HiBeaker />
              <HiCircleStack />
              <HiAdjustmentsHorizontal />
              <HiCog6Tooth />
            </div>

            <div className={styles.messageContentFooterRightSideIcons}>
              {enableDeleteBtn ? (
                <FaTrashAlt className={styles.messageContentFooterRightSideDeleteIconsBtn} onClick={handleDelete} />
              ) : (
                <div />
              )}
              {enableSwipeBtn ? (
                <div className={styles.messageContentFooterRightSideSwipeIcons}>
                  <label className={styles.messageContentFooterRightSideSwipeIconsLabel}>1/1</label>
                  <HiMiniChevronRight className={styles.messageContentFooterRightSideSwipeIconsBtn} />
                </div>
              ) : (
                <div />
              )}
            </div>
          </div>
        </div>
      </div>
      {isPromptModalOpen && (
        <div className={styles.modalOverlay} onClick={() => setIsPromptModalOpen(false)}>
          <div className={styles.modalBox} onClick={(e) => e.stopPropagation()}>
            <div className={styles.modalHeader}>
              <span className={styles.modalTitle}>Prompt</span>
              <button className={styles.modalCloseBtn} onClick={() => setIsPromptModalOpen(false)}>✕</button>
            </div>
            <div className={styles.modalBody}>
              {isLoadingPrompt ? (
                <span className={styles.modalLoading}>Loading…</span>
              ) : (
                <textarea
                  className={styles.modalTextarea}
                  readOnly
                  value={promptText ?? ""}
                />
              )}
            </div>
            <div className={styles.modalFooter}>
              <button
                className={styles.modalCopyBtn}
                onClick={handleCopyPrompt}
                disabled={!promptText || isLoadingPrompt}
              >
                {promptCopied ? "Copied!" : "Copy to clipboard"}
              </button>
            </div>
          </div>
        </div>
      )}
    </main>
  );
}
