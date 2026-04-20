import { useEffect, useState, useRef, useCallback } from "react";
import styles from "./InteractiveUserInputComponent.module.css";
import { HiUserPlus, HiCheckCircle, HiXCircle, HiSparkles } from "react-icons/hi2";
import { ImSpinner2 } from "react-icons/im";
import { getFromServerApiAsync, putToServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import { InteractiveUserInputStatus, InteractiveUserInputType, type InteractiveUserInputQueriesResponseDto, type InteractiveUserInputQueryResponse, type PutInteractiveUserInputQueryRequest } from "../../../../../ResponsesDto/chat/interactiveUserInput/InteractiveUserInputQueriesResponseDto";

interface Props {
  chatId: string;
  refreshToken?: number;
}

function getTypeLabel(type: InteractiveUserInputType): string {
  switch (type) {
    case InteractiveUserInputType.NewCharacterDetected:
      return "New character detected";
    default:
      return "Unknown";
  }
}

function getTypeIcon(type: InteractiveUserInputType) {
  switch (type) {
    case InteractiveUserInputType.NewCharacterDetected:
      return <HiUserPlus className={styles.queryTypeIcon} />;
    default:
      return <HiSparkles className={styles.queryTypeIcon} />;
  }
}

export default function InteractiveUserInputComponent({ chatId, refreshToken }: Props) {
  const [queries, setQueries] = useState<InteractiveUserInputQueryResponse[]>([]);
  const prevRefreshToken = useRef<number | undefined>(undefined);

  const fetchQueries = useCallback(async () => {
    if (!chatId) return;

    const response = await getFromServerApiAsync<InteractiveUserInputQueriesResponseDto>(
      `api/InteractiveUserInputQuery/chats/${chatId}`
    );

    const err = response as ServerApiExceptionResponseDto | null;
    if (!response || response.code !== 200 || err?.message) {
      console.error(`[InteractiveUserInput] Failed to fetch queries. Code:[${response?.code}]`);
      return;
    }

    const pending = (response.queries ?? []).filter(
      (q) => q.status === InteractiveUserInputStatus.WaitingUserInput
    );

    setQueries((prev) => {
      const existingIds = new Set(prev.map((q) => q.queryId));
      const merged = [...prev];
      for (const q of pending) {
        if (!existingIds.has(q.queryId)) merged.push(q);
      }
      return merged;
    });
  }, [chatId]);

  // Fetch on initial mount / page refresh
  useEffect(() => {
    fetchQueries();
  }, [fetchQueries]);

  // Fetch whenever a new AI response finishes
  useEffect(() => {
    if (refreshToken === undefined) return;
    if (refreshToken === prevRefreshToken.current) return;
    prevRefreshToken.current = refreshToken;
    fetchQueries();
  }, [refreshToken, fetchQueries]);

  const handleChoice = async (query: InteractiveUserInputQueryResponse, userChoice: boolean) => {
    // Immediately mark as Processing — removes it from the visible list
    setQueries((prev) =>
      prev.map((q) =>
        q.queryId === query.queryId
          ? { ...q, status: InteractiveUserInputStatus.Processing }
          : q
      )
    );

    const payload: PutInteractiveUserInputQueryRequest = {
      queryId: query.queryId,
      chatId: query.chatId,
      sceneTrackerId: query.sceneTrackerId,
      metadata: query.metadata,
      type: query.type,
      status: InteractiveUserInputStatus.Processing,
      userChoice,
    };

    const response = await putToServerApiAsync(`api/InteractiveUserInputQuery/${query.queryId}`, payload);
    const err = response as ServerApiExceptionResponseDto | null;

    if (!response || err?.message) {
      console.error(`[InteractiveUserInput] Failed to submit choice for queryId:[${query.queryId}]`);
      // Roll back to WaitingUserInput so the user can retry
      setQueries((prev) =>
        prev.map((q) =>
          q.queryId === query.queryId
            ? { ...q, status: InteractiveUserInputStatus.WaitingUserInput }
            : q
        )
      );
    }
  };

  const visibleQueries = queries.filter(
    (q) => q.status === InteractiveUserInputStatus.WaitingUserInput
  );

  if (visibleQueries.length === 0) return null;

  return (
    <div className={styles.interactiveUserInputComponent}>
      {visibleQueries.map((query) => (
        <div key={query.queryId} className={styles.queryRow}>
          <div className={styles.queryLeft}>
            {getTypeIcon(query.type)}
            <div className={styles.queryText}>
              <span className={styles.queryTypeLabel}>{getTypeLabel(query.type)}</span>
              {query.metadata && (
                <span className={styles.queryMetadata}>{query.metadata}</span>
              )}
            </div>
          </div>
          <div className={styles.queryActions}>
            <button
              className={`${styles.queryBtn} ${styles.queryBtnAccept}`}
              onClick={() => handleChoice(query, true)}
              title="Accept"
            >
              <HiCheckCircle />
              <span>Accept</span>
            </button>
            <button
              className={`${styles.queryBtn} ${styles.queryBtnRefuse}`}
              onClick={() => handleChoice(query, false)}
              title="Refuse"
            >
              <HiXCircle />
              <span>Refuse</span>
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
