import { useState, useRef, useEffect } from "react";
import type { ChatMessage } from "../../../../../ResponsesDto/chat/BusinessObjects/ChatMessage";
import styles from "./ChatMessageComponent.module.css";
import { HiAdjustmentsHorizontal, HiBeaker, HiMiniUsers, HiChatBubbleLeftEllipsis, HiCircleStack, HiCog6Tooth, HiIdentification, HiMiniChevronRight } from "react-icons/hi2";
import { GrRevert } from "react-icons/gr";
import { MdOutlineSummarize } from "react-icons/md";
import { FormatUtcDate } from "../../../../../utils/DateUtils";
import { HighlightedText } from "../../../../../utils/HighlightText";
import { GetAvatarPathFromCharacterId, GetAvatarPathFromPersonaId } from "../../../../../utils/avatarUtils";
import { FaTrashAlt } from "react-icons/fa";

interface Props {
  // messagesRef?: React.RefObject<HTMLDivElement | null>;
  message?: ChatMessage;
  defaultChatAvatarId?: string;
  enableDeleteBtn?: boolean;
  enableSwipeBtn?: boolean;
  isEditable?: boolean;
  onSave?: (messageId: string, newContent: string) => Promise<void>;
  onDelete?: (messageId: string) => Promise<void>;
}

export default function ChatMessageComponent({ message, defaultChatAvatarId, enableSwipeBtn = false,  enableDeleteBtn = false, isEditable = false, onSave, onDelete }: Props) {
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState(message?.content ?? "");
  const isRevertingRef = useRef(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // // Focus + size once when entering edit mode
  useEffect(() => {
    if (!isEditing || !textareaRef.current)
      return;

    const el = textareaRef.current;

    // Size to fit existing content (runs once, not on every keystroke)
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

    // setTimeout(() => {
    //   if (messagesRef?.current)
    //     messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    // }, 0);
  };

  const handleRevert = () => {
    isRevertingRef.current = true;
    setEditContent(message?.content ?? ""); // restore original
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
      return; // skip save
    }

    setIsEditing(false);
    const trimmed = editContent.trim();

    // Skip save if nothing changed
    if (!trimmed || trimmed === message?.content || !message?.messageId || !onSave)
      return;
    
    await onSave(message.messageId, trimmed);
  };

  return (
    <main className={styles.chatMessageComponent}>
      <div className={styles.container}>
        <div className={styles.leftMessageContainer}>
          <div className={styles.messageAvatarContainer}>
            {message?.sourceType == 0 ? (
              <img src={GetAvatarPathFromPersonaId(message?.personaId ?? "")} alt="Avatar" />
            ) : (
              <img src={GetAvatarPathFromCharacterId(message?.avatarId ?? defaultChatAvatarId ?? "")} alt="Avatar" />
            )}
          </div>
          <div className={styles.messageInfoContainer}>
            <div title="messageId">{!message?.messageIndex ? "-" : "# " + message.messageIndex}</div>
            {/* <div title="message generation time">??s</div>
            <div title="tokens in message">????t</div> */}
          </div>
        </div>
        <div className={styles.messageContent}>
          <div className={styles.messageHeaderContent}>
            <div className={styles.messageHeaderContentName}>
              {message?.sourceType == 0 ? <label>{message?.personaName ?? "User"}</label> : <label>{message?.characterName ?? "User"}</label>}
            </div>
            <div className={styles.messageHeaderContentModel}>model-name (?m??s)</div>
            <div className={styles.messageHeaderContentCreatedAt}>
              {message?.summarized ? (<MdOutlineSummarize className={styles.messageHeaderSummarizeIcon} title="Summarized" />) : ""}
              {message?.createdAtUtc ? FormatUtcDate(message.createdAtUtc) : ""}
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
                onMouseDown={(e) => e.preventDefault()} // prevents textarea blur firing first
                onClick={handleRevert}
              />
            </div>
          ) : (
            <div
              className={`${styles.messageContentValue} ${isEditable ? styles.messageContentValueEditable : ""}`}
              onDoubleClick={handleDoubleClick}
            >
              <HighlightedText text={message?.content ?? "[empty]"} />
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