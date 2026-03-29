import { useState, useRef, useEffect } from "react";
import type { ChatMessage } from "../../../../../ResponsesDto/chat/BusinessObjects/ChatMessage";
import styles from "./ChatMessageComponent.module.css";
import { HiAdjustmentsHorizontal, HiBeaker, HiMiniUsers, HiChatBubbleLeftEllipsis, HiCircleStack, HiCog6Tooth, HiIdentification, HiMiniChevronRight } from "react-icons/hi2";
import { GrRevert } from "react-icons/gr";
import { MdOutlineSummarize } from "react-icons/md";
import { ImSpinner2 } from "react-icons/im";
import { FormatUtcDate } from "../../../../../utils/DateUtils";
import { HighlightedText } from "../../../../../utils/HighlightText";
import { GetAvatarPathFromAvatarFilePath, GetAvatarPathFromChatIdAndAvatarId, GetAvatarPathFromPersonaId } from "../../../../../utils/avatarUtils";
import { FaTrashAlt } from "react-icons/fa";
import { getFromServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { BackgroundQueryResponseDto } from "../../../../../ResponsesDto/chat/BackgroundQueryResponseDto";
import type { BackgroundQueriesResponseDto } from "../../../../../ResponsesDto/chat/BackgroundQueriesResponseDto";
import type { ChatMessageResponseDto } from "../../../../../ResponsesDto/chat/ChatMessageResponseDto";

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
  const isRevertingRef = useRef(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Local copy of the message — updated after sceneAnalyze completes to reflect new avatar/content
  const [localMessage, setLocalMessage] = useState<ChatMessage | undefined>(message);

  // Keep localMessage in sync if parent re-renders with a new message (e.g. after an edit save)
  useEffect(() => {
    setLocalMessage(message);
  }, [message]);

  // --- sceneAnalyze polling state ---
  const [sceneAnalyzeQueryId, setSceneAnalyzeQueryId] = useState<string | null>(null);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const sceneAnalyzeNetworkErrorRef = useRef(0);

  // On mount: check if a sceneAnalyze background query is already in-flight for this chat
  useEffect(() => {
    if (!chatId)
      return;

    const abortController = new AbortController();

    const fetchSceneAnalyzeQuery = async () => {
      const response = await getFromServerApiAsync<BackgroundQueriesResponseDto>(
        `api/backgroundQueries?chatId=${chatId}`,
        abortController.signal
      );

      if (abortController.signal.aborted)
        return;

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code !== 200 || serverApiException?.message) {
        console.error(`[ChatMessageComponent] Fetching sceneAnalyze background queries failed. Code:[${response?.code}], Message:[${serverApiException?.message}]`);
        return;
      }

      if (!response.queries || response.queries.length <= 0)
        return;

      const sceneAnalyzeQuery = response.queries.find(query =>
        query.tags.some(tag => tag === "sceneAnalyze")
      );

      if (sceneAnalyzeQuery) {
        console.log("[ChatMessageComponent] Found in-flight sceneAnalyze query. Starting to poll for results...");
        setIsAnalyzing(true);
        setSceneAnalyzeQueryId(sceneAnalyzeQuery.backgroundQueryId);
      }
    };

    fetchSceneAnalyzeQuery();
    return () => abortController.abort();
  }, [chatId]);

  // Poll while a sceneAnalyze query is tracked
  useEffect(() => {
    if (!sceneAnalyzeQueryId)
      return;

    const pollInterval = setInterval(async () => {
      try {
        const response = await getFromServerApiAsync<BackgroundQueryResponseDto>(
          `api/backgroundQueries/${sceneAnalyzeQueryId}`
        );

        const serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code !== 200 || serverApiException?.message) {
          console.error(`[ChatMessageComponent] Polling sceneAnalyze query failed. Code:[${response?.code}], Message:[${serverApiException?.message}]`);

          if (sceneAnalyzeNetworkErrorRef.current >= 4) {
            clearInterval(pollInterval);
            setIsAnalyzing(false);
            setSceneAnalyzeQueryId(null);
            return;
          }

          sceneAnalyzeNetworkErrorRef.current += 1;
          return;
        }

        // Still running — stream partial content into localMessage if present
        if (response.status === "InProgress" || response.status === "Pending") {
          if (response.content) {
            setLocalMessage((prev) => prev ? { ...prev, content: response.content! } : prev);
          }
          return;
        }

        // Query is done — re-fetch the message by its own ID to get the refreshed avatar + content
        if (chatId && message?.messageId) {
          const refreshed = await getFromServerApiAsync<ChatMessageResponseDto>(
            `api/chat/${chatId}/messages/${message.messageId}`
          );

          const refreshedException = refreshed as ServerApiExceptionResponseDto | null;
          if (!refreshed || refreshedException?.message) {
            console.error(`[ChatMessageComponent] Re-fetching message after sceneAnalyze failed. [${JSON.stringify(refreshedException)}]`);
          } else if (refreshed.messageObj) {
            setLocalMessage(refreshed.messageObj);
          }
        }

        console.log("[ChatMessageComponent] sceneAnalyze generation complete.");
        sceneAnalyzeNetworkErrorRef.current = 0;
        clearInterval(pollInterval);
        setIsAnalyzing(false);
        setSceneAnalyzeQueryId(null);

      } catch (err) {
        console.error("[ChatMessageComponent] Polling sceneAnalyze error:", err);
        clearInterval(pollInterval);
        setIsAnalyzing(false);
      }
    }, 3000);

    return () => clearInterval(pollInterval);
  }, [sceneAnalyzeQueryId]);

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

    setEditContent(localMessage?.content ?? "");
    setIsEditing(true);
  };

  const handleRevert = () => {
    isRevertingRef.current = true;
    setEditContent(localMessage?.content ?? "");
    setIsEditing(false);
    isRevertingRef.current = false;
  };

  const handleDelete = async () => {
    if (!localMessage?.messageId || !onDelete)
      return;

    await onDelete(localMessage.messageId);
  };

  const handleBlur = async () => {
    if (isRevertingRef.current) {
      isRevertingRef.current = false;
      return;
    }

    setIsEditing(false);
    const trimmed = editContent.trim();

    if (!trimmed || trimmed === localMessage?.content || !localMessage?.messageId || !onSave)
      return;

    await onSave(localMessage.messageId, trimmed);
  };

  const displayedContent = localMessage?.content ?? "[empty]";

  return (
    <main className={styles.chatMessageComponent}>
      <div className={styles.container}>
        <div className={styles.leftMessageContainer}>
          <div className={styles.messageAvatarContainer}>
            {localMessage?.sourceType == 0 ? (
              <img src={GetAvatarPathFromPersonaId(localMessage?.personaId ?? "")} alt="Avatar" />
            ) : (
              <img src={localMessage?.avatarFilePath && localMessage.avatarFilePath !== "avatar" ? GetAvatarPathFromAvatarFilePath(localMessage.avatarFilePath) : GetAvatarPathFromChatIdAndAvatarId(chatId, "avatar")} alt="Avatar" />
            )}
          </div>
          <div className={styles.messageInfoContainer}>
            <div title="messageId">{!localMessage?.messageIndex ? "-" : "# " + localMessage.messageIndex}</div>
          </div>
        </div>
        <div className={styles.messageContent}>
          <div className={styles.messageHeaderContent}>
            <div className={styles.messageHeaderContentName}>
              {localMessage?.sourceType == 0 ? <label>{localMessage?.personaName ?? "User"}</label> : <label>{localMessage?.characterName ?? "User"}</label>}
            </div>
            <div className={styles.messageHeaderContentModel}>
              model-name (?m??s)
              {isAnalyzing && <ImSpinner2 className={styles.sceneAnalyzeSpinner} title="Scene analyzing..." />}
            </div>
            <div className={styles.messageHeaderContentCreatedAt}>
              {localMessage?.summarized ? (<MdOutlineSummarize className={styles.messageHeaderSummarizeIcon} title="Summarized" />) : ""}
              {localMessage?.createdAtUtc ? FormatUtcDate(localMessage.createdAtUtc) : ""}
            </div>
          </div>
          <div className={styles.messageContentSeparator} />

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
              <HiChatBubbleLeftEllipsis />
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
    </main>
  );
}
